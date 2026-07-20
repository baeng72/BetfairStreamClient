using System.Buffers;
using System.Buffers.Text;
using System.Text.Json;
using BetfairStreamClient.Logging;

namespace BetfairStreamClient.Stream
{
    public class StreamParser
    {
        private readonly MarketCacheManager _marketCache;
        private readonly OrderCacheManager _orderCache;

        private readonly Logger _logger;
        private DateTime _lastHeartbeat;

        public StreamParser(MarketCacheManager market, OrderCacheManager order, Logger logger)
        {
            _marketCache = market;
            _orderCache = order;
            _logger = logger;
        }

                public void ParseMessageNoAllocations(byte[] bytes, int length)
        {
            var reader = new Utf8JsonReader(bytes.AsSpan(0, length));
            bool isOrderMessage = false;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    if (reader.ValueTextEquals("op"))
                    {
                        reader.Read();
                        if (reader.ValueTextEquals("ct")) { _lastHeartbeat = DateTime.UtcNow; return; }
                        if (reader.ValueTextEquals("ocm")) 
                            isOrderMessage = true;
                        if (reader.ValueTextEquals("status"))
                        {
                            var statusReader = new Utf8JsonReader(bytes.AsSpan(0, length));
                            ParseAndLogStatusMessage(ref statusReader);
                            return; 
                        }
                    }
                    else if (reader.ValueTextEquals("mc") && !isOrderMessage)
                    {
                        ParseMarketChangesArray(ref reader);
                    }
                    else if (reader.ValueTextEquals("orc") && isOrderMessage)
                    {
                        ParseOrderChangesArray(ref reader);
                    }
                }
            }
        }
        private void ParseMarketChangesArray(ref Utf8JsonReader reader)
        {
            reader.Read(); 
            if (reader.TokenType != JsonTokenType.StartArray) return;

            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            {
                if (reader.TokenType == JsonTokenType.StartObject)
                {
                    string? currentMarketId = null;
                    bool hasPriceChanges = false;
                    bool isImageLoop = false; // Track image status first
                    MarketDefinition? freshDefinition = null;

                    // We must capture the raw unparsed reader window for 'rc' 
                    // because 'rc' might appear BEFORE we know if it's an image loop.
                    Utf8JsonReader deferredRunnerReader = default;

                    while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
                    {
                        if (reader.TokenType == JsonTokenType.PropertyName)
                        {
                            if (reader.ValueTextEquals("id"))
                            {
                                reader.Read();
                                if (reader.TokenType == JsonTokenType.String) currentMarketId = reader.GetString();
                            }
                            else if (reader.ValueTextEquals("marketDefinition"))
                            {
                                reader.Read();
                                freshDefinition = JsonSerializer.Deserialize<MarketDefinition>(ref reader);
                            }
                            else if (reader.ValueTextEquals("img"))
                            {
                                reader.Read();
                                isImageLoop = reader.GetBoolean();
                            }
                            else if (reader.ValueTextEquals("rc"))
                            {
                                // Clone the current reader state to parse the runner data later
                                deferredRunnerReader = reader; 
                                hasPriceChanges = true;
                                
                                // Skip the main reader past this entire array so it stays on track
                                reader.Read();
                                reader.Skip();
                            }
                            else
                            {
                                reader.Read(); 
                                reader.Skip(); 
                            }
                        }
                    }

                    // --- EXECUTION PHASE (ORDER ENFORCED) ---
                    if (currentMarketId != null)
                    {
                        // 1. Flush the old cache first if the image loop flag was present anywhere in the block
                        if (isImageLoop)
                        {
                            _marketCache.ClearCacheForMarket(currentMarketId);
                        }

                        // 2. Process runner changes using the deferred reader window
                        if (hasPriceChanges && deferredRunnerReader.TokenType != JsonTokenType.None)
                        {
                            ParseRunnerChanges(ref deferredRunnerReader, currentMarketId);
                        }

                        // 3. Broadcast the clean state
                        if (hasPriceChanges || freshDefinition != null)
                        {
                            _marketCache.ProcessAndBroadcast(currentMarketId, freshDefinition);
                        }
                    }
                }
            }
        }


        private void ParseRunnerChanges(ref Utf8JsonReader reader, string? marketId)
        {
            if (marketId == null || !reader.Read() || reader.TokenType != JsonTokenType.StartArray) return;

            // Loop through each runner object in the 'rc' array
            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            {
                if (reader.TokenType == JsonTokenType.StartObject)
                {
                    long selectionId = 0;
                    double lastTradedPrice = 0.0;
                    double tradedVolume = 0.0;

                    // Allocate lightweight snapshots on the stack frame
                    Utf8JsonReader bdatbReader = default;
                    Utf8JsonReader bdatlReader = default;
                    Utf8JsonReader batbReader  = default;
                    Utf8JsonReader batlReader  = default;
                    Utf8JsonReader atbReader   = default;
                    Utf8JsonReader atlReader   = default;
                    Utf8JsonReader trdReader = default;
                    

                    while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
                    {
                        if (reader.TokenType == JsonTokenType.PropertyName)
                        {
                            if (reader.ValueTextEquals("id"))
                            {
                                reader.Read();
                                if (reader.TokenType == JsonTokenType.Number) selectionId = reader.GetInt64();
                            }
                            else if (reader.ValueTextEquals("ltp"))
                            {
                                reader.Read();
                                if (reader.TokenType == JsonTokenType.Number)
                                {
                                    lastTradedPrice = reader.GetDouble();

                                }
                            }
                            else if (reader.ValueTextEquals("tv"))
                            {
                                reader.Read();
                                if (reader.TokenType == JsonTokenType.Number)
                                {
                                    tradedVolume = reader.GetDouble();
                                }
                            }
                        else if (reader.ValueTextEquals("bdatb")) { bdatbReader = reader; reader.Read(); reader.Skip(); }
                        else if (reader.ValueTextEquals("bdatl")) { bdatlReader = reader; reader.Read(); reader.Skip(); }
                        else if (reader.ValueTextEquals("batb")) { batbReader = reader; reader.Read(); reader.Skip(); }
                        else if (reader.ValueTextEquals("batl")) { batlReader = reader; reader.Read(); reader.Skip(); }
                        else if (reader.ValueTextEquals("atb")) { atbReader = reader; reader.Read(); reader.Skip(); }
                        else if (reader.ValueTextEquals("atl")) { atlReader = reader; reader.Read(); reader.Skip(); }
                        else if (reader.ValueTextEquals("trd")) { trdReader = reader; reader.Read(); reader.Skip(); }
                        else
                        {
                            reader.Read();
                            reader.Skip();
                        }
                        }
                    }

                    // --- DEFERRED PRICE STREAMING EXECUTION ---
                    if (selectionId != 0)
                    {
                        var cache = _marketCache.GetOrCreateRunnerCache(marketId, selectionId);
                        if(lastTradedPrice!=0.0)
                            cache.SetLastTradedPrice(lastTradedPrice);
                        if(tradedVolume!=0.0)
                            cache.SetTotalVolume(tradedVolume);   

                        if (bdatbReader.TokenType != JsonTokenType.None) StreamLadderDeltas(ref bdatbReader, marketId, selectionId, BetfairLadderType.Bdatb);
                        if (bdatlReader.TokenType != JsonTokenType.None) StreamLadderDeltas(ref bdatlReader, marketId, selectionId, BetfairLadderType.Bdatl);
                        if (batbReader.TokenType  != JsonTokenType.None) StreamLadderDeltas(ref batbReader,  marketId, selectionId, BetfairLadderType.Batb);
                        if (batlReader.TokenType  != JsonTokenType.None) StreamLadderDeltas(ref batlReader,  marketId, selectionId, BetfairLadderType.Batl);
                        if (atbReader.TokenType   != JsonTokenType.None) StreamLadderDeltas(ref atbReader,   marketId, selectionId, BetfairLadderType.Atb);
                        if (atlReader.TokenType   != JsonTokenType.None) StreamLadderDeltas(ref atlReader,   marketId, selectionId, BetfairLadderType.Atl);
                        if (trdReader.TokenType != JsonTokenType.None) StreamLadderDeltas(ref trdReader, marketId, selectionId, BetfairLadderType.Trd);
                    }
                }
            }
        }

        private void StreamLadderDeltas(ref Utf8JsonReader reader, string marketId, long selectionId, BetfairLadderType type)
        {
            reader.Read();
            bool isLevelPrizeSize = type == BetfairLadderType.Batl || type == BetfairLadderType.Batb ||
                type == BetfairLadderType.Bdatb || type == BetfairLadderType.Bdatl;
            var cache = _marketCache.GetOrCreateRunnerCache(marketId, selectionId);
            int i = 0;
            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            {
                if (reader.TokenType == JsonTokenType.StartArray) 
                {
                    reader.Read();
                    int level = i++;
                    if (isLevelPrizeSize)
                    {
                        level = (int)reader.GetDouble();
                        reader.Read();
                    }
                    double price = reader.GetDouble();
                    reader.Read(); 
                    double size = reader.GetDouble();
                    reader.Read();
                    
                    cache.UpdateSingleSlot(level, price, size, type);
                    
                }
            }
        }

        private void ParseOrderChangesArray(ref Utf8JsonReader reader)
        {
            reader.Read(); 
            if (reader.TokenType != JsonTokenType.StartArray) return;
            string? currentMarketId = null;

            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            {
                if (reader.TokenType == JsonTokenType.StartObject)
                {
                    bool isImageLoop = false;
                    bool hasOrderChanges = false;
                    Utf8JsonReader deferredOrderReader = default;

                    while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
                    {
                        if (reader.TokenType == JsonTokenType.PropertyName)
                        {
                            if (reader.ValueTextEquals("id"))
                            {
                                reader.Read();
                                if (reader.TokenType == JsonTokenType.String) 
                                    currentMarketId = reader.GetString();
                            }
                            else if (reader.ValueTextEquals("img"))
                            {
                                reader.Read();
                                isImageLoop = reader.GetBoolean();
                            }
                            else if (reader.ValueTextEquals("orc")) 
                            {
                                deferredOrderReader = reader;
                                hasOrderChanges = true;
                                
                                reader.Read();
                                reader.Skip();
                            }
                            else
                            {
                                reader.Read();
                                reader.Skip();
                            }
                        }
                    }

                    // --- EXECUTION PHASE ---
                    if (currentMarketId != null)
                    {
                        if (isImageLoop)
                        {
                            _orderCache.ClearCacheForMarket(currentMarketId);
                        }

                        if (hasOrderChanges && deferredOrderReader.TokenType != JsonTokenType.None)
                        {
                            ParseOrderRunnerChanges(ref deferredOrderReader, currentMarketId);
                        }

                        _orderCache.ProcessAndBroadcast(currentMarketId);
                    }
                }
            }
        }


        private void ParseOrderRunnerChanges(ref Utf8JsonReader reader, string? marketId)
        {
            if (marketId == null || !reader.Read() || reader.TokenType != JsonTokenType.StartArray) return;
            long selectionId = 0;

            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            {
                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    if (reader.ValueTextEquals("id")) 
                    { 
                        reader.Read(); 
                        if (reader.TokenType == JsonTokenType.Number) selectionId = reader.GetInt64(); 
                    }
                    else if (reader.ValueTextEquals("uo"))
                    {
                        StreamOrders(ref reader, marketId, selectionId);
                    }
                    else
                    {
                        reader.Read();
                        reader.Skip();
                    }
                }
            }
        }

        private void StreamOrders(ref Utf8JsonReader reader, string marketId, long selectionId)
        {
            reader.Read(); 
            var runnerCache = _orderCache.GetOrCreateRunnerCache(marketId, selectionId);
            Span<byte> betIdFallbackBuffer = stackalloc byte[32];

            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            {
                if (reader.TokenType == JsonTokenType.StartArray)
                {
                    long betId = 0; double p = 0; double sr = 0; double sm = 0; 
                    OrderSide side = OrderSide.Back;

                    while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                    {
                        if (reader.TokenType == JsonTokenType.PropertyName)
                        {
                            if (reader.ValueTextEquals("id"))
                            {
                                reader.Read();
                                if (reader.HasValueSequence)
                                {
                                    reader.ValueSequence.CopyTo(betIdFallbackBuffer);
                                    var span = betIdFallbackBuffer.Slice(0, (int)reader.ValueSequence.Length);
                                    Utf8Parser.TryParse(span, out betId, out _);
                                }
                                else
                                {
                                    Utf8Parser.TryParse(reader.ValueSpan, out betId, out _);
                                }
                            }
                            else if (reader.ValueTextEquals("p"))  { reader.Read(); p = reader.GetDouble(); }
                            else if (reader.ValueTextEquals("sr")) { reader.Read(); sr = reader.GetDouble(); }
                            else if (reader.ValueTextEquals("sm")) { reader.Read(); sm = reader.GetDouble(); }
                            else if (reader.ValueTextEquals("s"))
                            {
                                reader.Read(); 
                                side = reader.ValueTextEquals("B") ? OrderSide.Back : OrderSide.Lay;
                            }
                            else
                            {
                                reader.Read();
                                reader.Skip();
                            }
                        }
                    }
                    runnerCache.UpdateOrAddOrder(betId, p, sr, sm, side);
                }
            }
        }



        

        private void ParseAndLogStatusMessage(ref Utf8JsonReader reader)
        {
            string statusCode = "UNKNOWN";
            string errorCode = "NONE";
            string errorMessage = "No message provided";
            string connectionId = "NONE";

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    if (reader.ValueTextEquals("statusCode")) { reader.Read(); statusCode = reader.GetString() ?? "UNKNOWN"; }
                    else if (reader.ValueTextEquals("errorCode")) { reader.Read(); errorCode = reader.GetString() ?? "NONE"; }
                    else if (reader.ValueTextEquals("errorMessage")) { reader.Read(); errorMessage = reader.GetString() ?? ""; }
                    else if (reader.ValueTextEquals("connectionId")) { reader.Read(); connectionId = reader.GetString() ?? "NONE"; }
                }
            }

            // Print out the precise structural error detail to your console or diagnostic system
            
            _logger.Log($"[Betfair Stream Error] Connection ID: {connectionId}");
            _logger.Log($"Status Code: {statusCode}");
            _logger.Log($"Error Code: {errorCode}");
            _logger.Log($"Details: {errorMessage}");
            

            // Trigger your recovery systems or stop the socket connection loop here if unauthorized
        }


        
        
    }
}
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
                        if (reader.ValueTextEquals("ocm")) isOrderMessage = true;
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
                    MarketDefinition? freshDefinition = null;

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
                                if (reader.GetBoolean() && currentMarketId != null) _marketCache.ClearCacheForMarket(currentMarketId);
                            }
                            else if (reader.ValueTextEquals("rc"))
                            {
                                ParseRunnerChanges(ref reader, currentMarketId);
                                hasPriceChanges = true;
                            }
                            else
                            {
                                reader.Read(); 
                                reader.Skip(); 
                            }
                        }
                    }

                    if (currentMarketId != null && (hasPriceChanges || freshDefinition != null))
                    {
                        _marketCache.ProcessAndBroadcast(currentMarketId, freshDefinition);
                    }
                }
            }
        }

        private void ParseRunnerChanges(ref Utf8JsonReader reader, string? marketId)
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
                    else if (reader.ValueTextEquals("bdatb")) StreamLadderDeltas(ref reader, marketId, selectionId, BetfairLadderType.Bdatb);
                    else if (reader.ValueTextEquals("bdatl")) StreamLadderDeltas(ref reader, marketId, selectionId, BetfairLadderType.Bdatl);
                    else if (reader.ValueTextEquals("batb"))  StreamLadderDeltas(ref reader, marketId, selectionId, BetfairLadderType.Batb);
                    else if (reader.ValueTextEquals("batl"))  StreamLadderDeltas(ref reader, marketId, selectionId, BetfairLadderType.Batl);
                    else if (reader.ValueTextEquals("atb"))   StreamLadderDeltas(ref reader, marketId, selectionId, BetfairLadderType.Atb);
                    else if (reader.ValueTextEquals("atl"))   StreamLadderDeltas(ref reader, marketId, selectionId, BetfairLadderType.Atl);
                    else
                    {
                        reader.Read();
                        reader.Skip();
                    }
                }
            }
        }

        private void StreamLadderDeltas(ref Utf8JsonReader reader, string marketId, long selectionId, BetfairLadderType type)
        {
            reader.Read(); 
            var cache = _marketCache.GetOrCreateRunnerCache(marketId, selectionId);

            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            {
                if (reader.TokenType == JsonTokenType.StartArray) 
                {
                    reader.Read(); int level = (int)reader.GetDouble();
                    reader.Read(); double price = reader.GetDouble();
                    reader.Read(); double size = reader.GetDouble();
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
                    while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
                    {
                        if (reader.TokenType == JsonTokenType.PropertyName)
                        {
                            if (reader.ValueTextEquals("id"))
                            {
                                reader.Read();
                                if (reader.TokenType == JsonTokenType.String) currentMarketId = reader.GetString();
                            }
                            else if (reader.ValueTextEquals("img"))
                            {
                                reader.Read();
                                if (reader.GetBoolean() && currentMarketId != null) _orderCache.ClearCacheForMarket(currentMarketId);
                            }
                            else if (reader.ValueTextEquals("orc")) 
                            {
                                ParseOrderRunnerChanges(ref reader, currentMarketId);
                            }
                            else
                            {
                                reader.Read();
                                reader.Skip();
                            }
                        }
                    }

                    if (currentMarketId != null)
                    {
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
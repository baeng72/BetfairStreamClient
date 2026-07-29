using BetfairStreamClient.Logging;
using System;
using System.Buffers;
using System.Buffers.Text;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Transactions;

namespace BetfairStreamClient.ExchangeStream
{
    public class StreamParser<T, TSnap> where T : struct, IDisposable, IClearable where TSnap : struct, IDisposable, IClearable
    {
        private readonly MarketCacheManager<T,TSnap> _marketCacheManager;
        private readonly OrderCacheManager _orderCacheManager;

        private readonly Logger _logger;
        private DateTime _lastHeartbeat;

        public StreamParser(MarketCacheManager<T, TSnap> marketCacheManager, OrderCacheManager orderCacheManager, Logger logger)
        {
            _marketCacheManager = marketCacheManager;
            _orderCacheManager = orderCacheManager;
            _logger = logger;
        }

        public void ParseMessageNoAllocations(byte[] bytes, int length)
        {
            var reader = new Utf8JsonReader(bytes.AsSpan(0, length));
            bool isOrderMessage = false;
            DateTime timeStamp = DateTime.UtcNow;
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    string propName = reader.GetString();
                    if (reader.ValueTextEquals("op"u8))
                    {
                        reader.Read();
                        if (reader.ValueTextEquals("ct"u8)) { _lastHeartbeat = DateTime.UtcNow; return; }
                        if (reader.ValueTextEquals("ocm"u8)) 
                            isOrderMessage = true;
                        if (reader.ValueTextEquals("status"u8))
                        {
                            var statusReader = new Utf8JsonReader(bytes.AsSpan(0, length));
                            ParseAndLogStatusMessage(ref statusReader);
                            return; 
                        }
                    }
                    else if (reader.ValueTextEquals("mc"u8) && !isOrderMessage)
                    {
                        ParseMarketChangesArray(ref reader, timeStamp);
                    }
                    else if (reader.ValueTextEquals("oc"u8) && isOrderMessage)
                    {
                        ParseOrderChangesArray(ref reader, timeStamp);
                    }
                    else if (reader.ValueTextEquals("pt"u8))
                    {
                        reader.Read();
                        if (reader.TokenType != JsonTokenType.Null)
                            timeStamp = convertUnixToDateTime(reader.GetInt64());
                    }
                }
            }
        }
        private void ParseMarketChangesArray(ref Utf8JsonReader reader, DateTime timeStamp)
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
                    double totalVolume = 0.0;                    
                    // We must capture the raw unparsed reader window for 'rc' 
                    // because 'rc' might appear BEFORE we know if it's an image loop.
                    Utf8JsonReader deferredRunnerReader = default;

                    while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
                    {
                        if (reader.TokenType == JsonTokenType.PropertyName)
                        {
                            var propName = reader.GetString();
                            //string propertyName = reader.GetString();
                            if (reader.ValueTextEquals("id"u8))
                            {
                                reader.Read();
                                if (reader.TokenType == JsonTokenType.String)
                                {
                                    currentMarketId = reader.GetString();

                                }
                            }
                            else if (reader.ValueTextEquals("marketDefinition"))
                            {
                                reader.Read();
                                freshDefinition = JsonSerializer.Deserialize<MarketDefinition>(ref reader);
                            }
                            else if (reader.ValueTextEquals("img"u8))
                            {
                                reader.Read();
                                if(reader.TokenType != JsonTokenType.Null)
                                    isImageLoop = reader.GetBoolean();
                            }
                            else if (reader.ValueTextEquals("rc"u8))
                            {
                                // Clone the current reader state to parse the runner data later
                                deferredRunnerReader = reader;
                                hasPriceChanges = true;

                                // Skip the main reader past this entire array so it stays on track
                                reader.Read();
                                reader.Skip();
                            }
                            else if (reader.ValueTextEquals("tv"u8))
                            {
                                reader.Read();
                                if (reader.TokenType != JsonTokenType.Null)
                                    totalVolume = reader.GetDouble();
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
                        var marketCache = _marketCacheManager.GetOrCreateMarket(currentMarketId);


                        //    // 1. Flush the old cache first if the image loop flag was present anywhere in the block
                        if (isImageLoop)
                        {
                            marketCache.Clear();
                        }

                        //    // 2. Process runner changes using the deferred reader window
                        if (hasPriceChanges && deferredRunnerReader.TokenType != JsonTokenType.None)
                        {

                            ParseRunnerChanges(ref deferredRunnerReader, marketCache, timeStamp);
                        }

                        //    // 3. Broadcast the clean state
                        if (hasPriceChanges || freshDefinition != null)
                        {
                            _marketCacheManager.ProcessAndBroadcast(currentMarketId, timeStamp, freshDefinition);
                        }
                    }
                }
            }
        }

        private void ParseRunnerChanges(ref Utf8JsonReader reader, MarketCache<T, TSnap> marketCache, DateTime timeStamp)
        {
            if (marketCache == null || !reader.Read() || reader.TokenType != JsonTokenType.StartArray) return;
            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            {
                if (reader.TokenType == JsonTokenType.StartObject)
                {
                    // Allocate lightweight snapshots on the stack frame
                    Utf8JsonReader bdatbReader = default;
                    Utf8JsonReader bdatlReader = default;
                    Utf8JsonReader batbReader = default;
                    Utf8JsonReader batlReader = default;
                    Utf8JsonReader atbReader = default;
                    Utf8JsonReader atlReader = default;
                    Utf8JsonReader spbReader = default;
                    Utf8JsonReader splReader = default;
                    Utf8JsonReader trdReader = default;
                    long selectionId = 0;
                    double tradedVolume = -1.0;
                    double lastTradedPrice = -1.0;
                    double startingPriceNear = -1.0;
                    double startingPriceFar = -1.0;
                    bool processTraded = false;
                    bool processBatb = false;
                    bool processBatl = false;
                    bool processBdatb = false;
                    bool processBdatl = false;
                    bool processAtb = false;
                    bool processAtl = false;

                    while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
                    {
                        if (reader.TokenType == JsonTokenType.PropertyName)
                        {
                            string propertyName = reader.GetString();

                            if (reader.ValueTextEquals("id"u8))
                            {
                                reader.Read();
                                if (reader.TokenType != JsonTokenType.Null)
                                    selectionId = reader.GetInt64();
                            }
                            else if (reader.ValueTextEquals("tv"u8))
                            {
                                reader.Read();
                                if (reader.TokenType != JsonTokenType.Null)
                                    tradedVolume = reader.GetDouble();
                            }
                            else if (reader.ValueTextEquals("ltp"u8))
                            {
                                reader.Read();
                                if (reader.TokenType != JsonTokenType.Null)
                                    lastTradedPrice = reader.GetDouble();
                            }
                            else if (reader.ValueTextEquals("spn"u8))
                            {
                                reader.Read();
                                if (reader.TokenType != JsonTokenType.Null)
                                    startingPriceNear = reader.GetDouble();
                            }
                            else if (reader.ValueTextEquals("spf"u8))
                            {
                                reader.Read();
                                if(reader.TokenType!= JsonTokenType.Null)
                                    startingPriceFar = reader.GetDouble();
                            }
                            else if (reader.ValueTextEquals("batb"u8))
                            {
                                batbReader = reader;
                                reader.Read();
                                processBatb = reader.TokenType != JsonTokenType.Null;
                                reader.Skip();
                            }
                            else if (reader.ValueTextEquals("batl"u8))
                            {
                                batlReader = reader;
                                reader.Read();
                                processBatl = reader.TokenType != JsonTokenType.Null;
                                reader.Skip();
                            }
                            else if (reader.ValueTextEquals("bdatb"u8))
                            {
                                bdatbReader = reader;
                                reader.Read();
                                processBdatb = reader.TokenType != JsonTokenType.Null;
                                reader.Skip();
                            }
                            else if (reader.ValueTextEquals("bdatl"u8))
                            {
                                bdatlReader = reader;
                                reader.Read();
                                processBdatl = reader.TokenType != JsonTokenType.Null;
                                reader.Skip();
                            }
                            else if (reader.ValueTextEquals("atb"u8))
                            {
                                atbReader = reader;
                                reader.Read();
                                processAtb = reader.TokenType != JsonTokenType.Null;
                                reader.Skip();
                            }
                            else if (reader.ValueTextEquals("atl"u8))
                            {
                                atlReader = reader;
                                reader.Read();
                                processAtl = reader.TokenType != JsonTokenType.Null;
                                reader.Skip();
                            }
                            else if (reader.ValueTextEquals("spb"u8))
                            {
                                spbReader = reader;
                                reader.Read();
                                reader.Skip();
                            }
                            else if (reader.ValueTextEquals("spl"u8))
                            {
                                splReader = reader;
                                reader.Read();
                                reader.Skip();
                            }
                            else if (reader.ValueTextEquals("trd"u8))
                            {
                                trdReader = reader;
                                reader.Read();                                
                                processTraded = reader.TokenType != JsonTokenType.Null;
                                reader.Skip();
                            }
                            else
                            {
                                reader.Read();
                                reader.Skip();
                            }
                        }
                    }
                    if (selectionId != 0)
                    {
                        var runner = marketCache.GetOrCreateRunner(selectionId);
                        runner.SelectionId = selectionId;
                        if (typeof(T) == typeof(MarketRunnerBdat))
                        {
                            var runnerBdat = (MarketRunner<MarketRunnerBdat>)((object)runner);
                            ref MarketRunnerBdat marketRunner = ref runnerBdat.RunnerData;
                            if (bdatbReader.TokenType != JsonTokenType.None && processBdatb) StreamLevelDeltas(ref bdatbReader, ref marketRunner.BestDisplayAvailableToBack, ref marketRunner.BestDisplayAvailableToBackCount);
                            if (bdatlReader.TokenType != JsonTokenType.None && processBdatl) StreamLevelDeltas(ref bdatlReader, ref marketRunner.BestDisplayAvailableToLay, ref marketRunner.BestDisplayAvailableToLayCount);
                        }
                        else if (typeof(T) == typeof(MarketRunnerBdatTraded))
                        {
                            var runnerBdat = (MarketRunner<MarketRunnerBdatTraded>)((object)runner);
                            ref MarketRunnerBdatTraded marketRunner = ref runnerBdat.RunnerData;
                            if (bdatbReader.TokenType != JsonTokenType.None && processBdatb) StreamLevelDeltas(ref bdatbReader, ref marketRunner.BestDisplayAvailableToBack, ref marketRunner.BestDisplayAvailableToBackCount);
                            if (bdatlReader.TokenType != JsonTokenType.None && processBdatl) StreamLevelDeltas(ref bdatlReader, ref marketRunner.BestDisplayAvailableToLay, ref marketRunner.BestDisplayAvailableToLayCount);
                            if (trdReader.TokenType != JsonTokenType.None && processTraded) StreamPriceSizeDeltas(ref trdReader, ref marketRunner.Traded, ref marketRunner.TradedCount);
                        }
                        else if (typeof(T) == typeof(MarketRunnerBat))
                        {
                            var runnerBat = (MarketRunner<MarketRunnerBat>)((object)runner);
                            ref MarketRunnerBat marketRunner = ref runnerBat.RunnerData;
                            if (batbReader.TokenType != JsonTokenType.None && processBatb) StreamLevelDeltas(ref batbReader, ref marketRunner.BestAvailableToBack, ref marketRunner.BestAvailableToBackCount);
                            if (batlReader.TokenType != JsonTokenType.None && processBatl) StreamLevelDeltas(ref batlReader, ref marketRunner.BestAvailableToLay, ref marketRunner.BestAvailableToLayCount);
                        }
                        else if (typeof(T) == typeof(MarketRunnerBatTraded))
                        {
                            var runnerBat = (MarketRunner<MarketRunnerBatTraded>)((object)runner);
                            ref MarketRunnerBatTraded marketRunner = ref runnerBat.RunnerData;
                            if (batbReader.TokenType != JsonTokenType.None && processBatb) StreamLevelDeltas(ref batbReader, ref marketRunner.BestAvailableToBack, ref marketRunner.BestAvailableToBackCount);
                            if (batlReader.TokenType != JsonTokenType.None && processBatl) StreamLevelDeltas(ref batlReader, ref marketRunner.BestAvailableToLay, ref marketRunner.BestAvailableToLayCount);
                            if (trdReader.TokenType != JsonTokenType.None && processTraded) StreamPriceSizeDeltas(ref trdReader, ref marketRunner.Traded, ref marketRunner.TradedCount);
                        }
                        else if (typeof(T) == typeof(MarketRunnerAt))
                        {
                            var runnerBat = (MarketRunner<MarketRunnerAt>)((object)runner);
                            ref MarketRunnerAt marketRunner = ref runnerBat.RunnerData;
                            if (atbReader.TokenType != JsonTokenType.None && processAtb) StreamPriceSizeDeltas(ref atbReader, ref marketRunner.AvailableToBack, ref marketRunner.AvailableToBackCount);
                            if (atlReader.TokenType != JsonTokenType.None && processAtl) StreamPriceSizeDeltas(ref atlReader, ref marketRunner.AvailableToLay, ref marketRunner.AvailableToLayCount);
                        }
                        else if (typeof(T) == typeof(MarketRunnerAtTraded))
                        {
                            var runnerBat = (MarketRunner<MarketRunnerAtTraded>)((object)runner);
                            ref MarketRunnerAtTraded marketRunner = ref runnerBat.RunnerData;
                            if (atbReader.TokenType != JsonTokenType.None && processAtb) StreamPriceSizeDeltas(ref atbReader, ref marketRunner.AvailableToBack, ref marketRunner.AvailableToBackCount);
                            if (atlReader.TokenType != JsonTokenType.None && processAtl) StreamPriceSizeDeltas(ref atlReader, ref marketRunner.AvailableToLay, ref marketRunner.AvailableToLayCount);
                            if (trdReader.TokenType != JsonTokenType.None) StreamPriceSizeDeltas(ref trdReader, ref marketRunner.Traded, ref marketRunner.TradedCount);
                        }
                        else if (typeof(T) == typeof(MarketRunnerTraded))
                        {
                            var runnerTraded = (MarketRunner<MarketRunnerTraded>)((object)runner);
                            ref MarketRunnerTraded marketRunner = ref runnerTraded.RunnerData;
                            if (trdReader.TokenType != JsonTokenType.None && processTraded) StreamPriceSizeDeltas(ref trdReader, ref marketRunner.Traded, ref marketRunner.TradedCount);
                        }
                        else if (typeof(T) == typeof(MarketRunnerLastTradedPrice))
                        {
                            var runnerTraded = (MarketRunner<MarketRunnerLastTradedPrice>)((object)runner);
                            ref MarketRunnerLastTradedPrice marketRunner = ref runnerTraded.RunnerData;
                            if (lastTradedPrice > 0.0)
                                marketRunner.LastTradedPrice = lastTradedPrice;
                        }
                        else if (typeof(T) == typeof(MarketRunnerTradedVolume))
                        {
                            var runnerTraded = (MarketRunner<MarketRunnerTradedVolume>)((object)runner);
                            ref MarketRunnerTradedVolume marketRunner = ref runnerTraded.RunnerData;
                            if (tradedVolume > 0.0)
                                marketRunner.TradedVolume = tradedVolume;
                        }
                        else if (typeof(T) == typeof(MarketRunnerBatTradedTVLTP))
                        {
                            var runnerBat = (MarketRunner<MarketRunnerBatTradedTVLTP>)((object)runner);
                            ref MarketRunnerBatTradedTVLTP marketRunner = ref runnerBat.RunnerData;
                            if (batbReader.TokenType != JsonTokenType.None && processBatb) StreamLevelDeltas(ref batbReader, ref marketRunner.BestAvailableToBack, ref marketRunner.BestAvailableToBackCount);
                            if (batlReader.TokenType != JsonTokenType.None && processBatl) StreamLevelDeltas(ref batlReader, ref marketRunner.BestAvailableToLay, ref marketRunner.BestAvailableToLayCount);
                            if (trdReader.TokenType != JsonTokenType.None && processTraded) StreamPriceSizeDeltas(ref trdReader, ref marketRunner.Traded, ref marketRunner.TradedCount);
                            if (tradedVolume > 0.0)
                                marketRunner.TradedVolume = tradedVolume;
                            if (lastTradedPrice > 0.0)
                                marketRunner.LastTradedPrice = lastTradedPrice;
                        }
                        else if (typeof(T) == typeof(MarketRunnerBatTVLTP))
                        {
                            var runnerBat = (MarketRunner<MarketRunnerBatTVLTP>)((object)runner);
                            ref MarketRunnerBatTVLTP marketRunner = ref runnerBat.RunnerData;
                            if (batbReader.TokenType != JsonTokenType.None && processBatb) StreamLevelDeltas(ref batbReader, ref marketRunner.BestAvailableToBack, ref marketRunner.BestAvailableToBackCount);
                            if (batlReader.TokenType != JsonTokenType.None && processBatl) StreamLevelDeltas(ref batlReader, ref marketRunner.BestAvailableToLay, ref marketRunner.BestAvailableToLayCount);
                            if (tradedVolume > 0.0)
                                marketRunner.TradedVolume = tradedVolume;
                            if (lastTradedPrice > 0.0)
                                marketRunner.LastTradedPrice = lastTradedPrice;
                        }
                        else if (typeof(T) == typeof(MarketRunnerAtTradedTVLTP))
                        {
                            var runnerBat = (MarketRunner<MarketRunnerAtTradedTVLTP>)((object)runner);
                            ref MarketRunnerAtTradedTVLTP marketRunner = ref runnerBat.RunnerData;
                            if (atbReader.TokenType != JsonTokenType.None && processAtb) StreamPriceSizeDeltas(ref atbReader, ref marketRunner.AvailableToBack, ref marketRunner.AvailableToBackCount);
                            if (atlReader.TokenType != JsonTokenType.None && processAtl) StreamPriceSizeDeltas(ref atlReader, ref marketRunner.AvailableToLay, ref marketRunner.AvailableToLayCount);
                            if (trdReader.TokenType != JsonTokenType.None && processTraded) StreamPriceSizeDeltas(ref trdReader, ref marketRunner.Traded, ref marketRunner.TradedCount);
                            if (tradedVolume > 0.0)
                                marketRunner.TradedVolume = tradedVolume;
                            if (lastTradedPrice > 0.0)
                                marketRunner.LastTradedPrice = lastTradedPrice;
                        }
                    }
                }
            }
        }

        private void StreamLevelDeltas(ref Utf8JsonReader reader, ref LevelPriceSize[] levelDeltas, ref int count)
        {
            reader.Read();
            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            {
                if (reader.TokenType == JsonTokenType.StartArray)
                {
                    //FIX THIS: when size == 0, then this needs to be **removed**, could mean all following items are empty. 
                    //Best way to handle at moment, is iterate through, when size==0, we're done?
                    reader.Read();
                    int level = (int)reader.GetDouble();
                    if (level + 1 > count)
                        count = level + 1;
                    reader.Read();
                    double price = reader.GetDouble();
                    reader.Read();
                    double size = reader.GetDouble();
                    if (size == 0.0)
                    {
                        count = level + 1;
                    }
                    reader.Read();
                    levelDeltas[level] = new LevelPriceSize(level, price, size);
                }
            }
        }
        private void StreamLevelDeltas(ref Utf8JsonReader reader, ref LevelPriceSizeCache levelDeltas, ref int count)
        {
            reader.Read();
            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            {
                if (reader.TokenType == JsonTokenType.StartArray)
                {
                    reader.Read();
                    int level = (int)reader.GetDouble();
                    reader.Read();
                    double price = reader.GetDouble();
                    reader.Read();
                    double size = reader.GetDouble();
                    
                    reader.Read();
                    levelDeltas.Update(level, price, size);                    
                }

            }
            count = levelDeltas.Count;
        }

        
        private void StreamPriceSizeDeltas(ref Utf8JsonReader reader, ref PriceSizeLadder priceSizeLadder, ref int count)
        {
            reader.Read();
            if (reader.TokenType != JsonTokenType.StartArray) return;
            
            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            {
                if (reader.TokenType == JsonTokenType.StartArray)
                {
                    reader.Read();
                    double price = reader.GetDouble();
                    reader.Read();
                    double size = reader.GetDouble();
                    reader.Read();
                    priceSizeLadder.Update(price, size);                    
                }
            
            }
            count = priceSizeLadder.LadderCount;//keep copy, might be wastefull,as PriceSizeLadder keeps this count 
            
        }


        private void ParseOrderChangesArray(ref Utf8JsonReader reader, DateTime timeStamp)
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
                            string propertyName = reader.GetString();
                            if (reader.ValueTextEquals("id"))
                            {
                                reader.Read();
                                if (reader.TokenType == JsonTokenType.String)
                                    currentMarketId = reader.GetString();
                            }
                            else if (reader.ValueTextEquals("fullImage"))
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
                        var orderCache = _orderCacheManager.GetOrCreateMarket(currentMarketId);
                        if (isImageLoop)
                        {
                            orderCache.Clear();
                        }

                        if (hasOrderChanges && deferredOrderReader.TokenType != JsonTokenType.None)
                        {
                            ParseOrderRunnerChanges(ref deferredOrderReader, orderCache);
                        }
                        if (hasOrderChanges)
                            _orderCacheManager.ProcessAndBroadcast(currentMarketId, timeStamp);
                    }
                }
            }
        }

        private void ParseOrderRunnerChanges(ref Utf8JsonReader reader, OrderMarketCache marketCache)
        {
            if (marketCache == null || !reader.Read() || reader.TokenType != JsonTokenType.StartArray) return;
            long selectionId = 0;
            bool fullImage = false;
            string marketId = marketCache.MarketId;
            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            {
                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    string val = reader.GetString();
                    if (reader.ValueTextEquals("fullImage"u8))
                    {
                        reader.Read();
                        fullImage = reader.GetBoolean();
                    }
                    else if (reader.ValueTextEquals("id"u8))
                    {
                        reader.Read();
                        if (reader.TokenType == JsonTokenType.Number)
                        {
                            selectionId = reader.GetInt64();
                            ref OrderRunnerCache runnerCache = ref marketCache.GetOrCreateRunnerCache(marketId, selectionId);
                            runnerCache.Clear();
                        }
                    }
                    else if (reader.ValueTextEquals("uo"u8))
                    {

                        StreamOrders(ref reader, marketId, selectionId, marketCache);
                    }
                    else if (reader.ValueTextEquals("mb"u8))
                    {
                        //Matched back
                        StreamOrderDeltas(ref reader, marketId, selectionId, true, marketCache);
                    }
                    else if (reader.ValueTextEquals("ml"u8))
                    {
                        StreamOrderDeltas(ref reader, marketId, selectionId, false, marketCache);
                    }
                    else
                    {
                        reader.Read();
                        reader.Skip();
                    }
                }
            }
        }
        private void StreamOrderDeltas(ref Utf8JsonReader reader, string marketId, long selectionId, bool isBack, OrderMarketCache marketCache)
        {
            reader.Read();
            ref OrderRunnerCache runnerCache = ref marketCache.GetOrCreateRunnerCache(marketId, selectionId);
            int i = 0;
            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            {
                if (reader.TokenType == JsonTokenType.StartArray)
                {
                    reader.Read();
                    double price = reader.GetDouble();
                    reader.Read();
                    double size = reader.GetDouble();

                    reader.Read();
                    if (isBack)
                        runnerCache.AddMatchedBacks(price, size);
                    else
                        runnerCache.AddMatchedLays(price, size);

                }
            }
        }
        DateTime convertUnixToDateTime(long unixMilliseconds)
        {
            //const long unixMilliseconds = 1711929600000;

            // Returns a UTC DateTime
            DateTime dateTimeUtc = DateTimeOffset.FromUnixTimeMilliseconds(unixMilliseconds).UtcDateTime;
            return dateTimeUtc;

        }
        private void StreamOrders(ref Utf8JsonReader reader, string marketId, long selectionId, OrderMarketCache marketCache)
        {
            //reader.Read();
            var runnerCache = marketCache.GetOrCreateRunnerCache(marketId, selectionId);
            Span<byte> betIdFallbackBuffer = stackalloc byte[32];
            
            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            {
                if (reader.Read() && reader.TokenType == JsonTokenType.StartObject)
                {
                    string betId = ""; string rfo = ""; string rfs = "";
                    double p = 0; double sl = 0.0; double sc = 0.0;
                    double sr = 0.0; double sm = 0.0; double sv = 0.0; double s = 0.0;
                    double avp = 0.0;
                    SideEnum side = SideEnum.Back;
                    OrderTypeEnum ot = OrderTypeEnum.LIMIT;
                    PersistenceTypeEnum pt = PersistenceTypeEnum.LAPSE;
                    OrderStatusEnum status = OrderStatusEnum.EXECUTABLE;
                    DateTime matchedDate = DateTime.MinValue;
                    DateTime cancelledDate = DateTime.MinValue;
                    DateTime placedDate = DateTime.MinValue;
                    
                    while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
                    {


                        if (reader.TokenType == JsonTokenType.PropertyName)
                        {
                            string propertyName = reader.GetString();
                            if (reader.ValueTextEquals("id"u8))
                            {
                                reader.Read();
                                if (reader.HasValueSequence)
                                {
                                    reader.ValueSequence.CopyTo(betIdFallbackBuffer);
                                    var span = betIdFallbackBuffer.Slice(0, (int)reader.ValueSequence.Length);
                                    //Utf8Parser.TryParse(span, out betId, out _);
                                    betId = span.ToString();
                                }
                                else
                                {
                                    //Utf8Parser.TryParse(reader.ValueSpan, out betId, out _);
                                    betId = reader.GetString();
                                }
                            }
                            else if (reader.ValueTextEquals("s"u8))
                            {
                                reader.Read();
                                s = reader.GetDouble();
                            }
                            else if (reader.ValueTextEquals("p"u8))
                            {
                                reader.Read();
                                p = reader.GetDouble();
                            }
                            else if (reader.ValueTextEquals("sr"u8))
                            {
                                reader.Read();
                                sr = reader.GetDouble();
                            }
                            else if (reader.ValueTextEquals("sm"u8))
                            {
                                reader.Read();
                                sm = reader.GetDouble();
                            }
                            else if (reader.ValueTextEquals("sv"u8))
                            {
                                reader.Read();
                                sv = reader.GetDouble();
                            }
                            else if (reader.ValueTextEquals("sl"u8))
                            {
                                reader.Read();
                                sl = reader.GetDouble();
                            }
                            else if (reader.ValueTextEquals("sc"u8))
                            {
                                reader.Read();
                                sc = reader.GetDouble();
                            }
                            else if (reader.ValueTextEquals("avp"u8))
                            {
                                reader.Read();
                                avp = reader.GetDouble();
                            }
                            else if (reader.ValueTextEquals("pt"u8))
                            {
                                reader.Read();
                                
                                var val = reader.GetString();
                                switch (val)
                                {
                                    case "L":
                                        pt = PersistenceTypeEnum.LAPSE;
                                        break;
                                    case "P":
                                        pt = PersistenceTypeEnum.PERSIST;
                                        break;
                                    case "MOC":
                                        pt = PersistenceTypeEnum.MARKET_ON_CHANGE;
                                        break;
                                }
                            }
                            else if (reader.ValueTextEquals("ot"u8))
                            {
                                reader.Read();
                                var val = reader.GetString();
                                switch (val)
                                {
                                    case "L":
                                        ot = OrderTypeEnum.LIMIT;
                                        break;
                                    case "LOC":
                                        ot = OrderTypeEnum.LIMIT_ON_CLOSE;
                                        break;
                                    case "MOC":
                                        ot = OrderTypeEnum.MARKET_ON_CLOSE;
                                        break;
                                }
                            }

                            else if (reader.ValueTextEquals("side"u8))
                            {
                                reader.Read();
                                var val = reader.GetString();
                                switch (val)
                                {
                                    case "B":
                                        side = SideEnum.Back;
                                        break;
                                    case "L":
                                        side = SideEnum.Lay;
                                        break;
                                }
                            }
                            else if (reader.ValueTextEquals("status"u8))
                            {
                                reader.Read();
                                var val = reader.GetString();
                                switch (val)
                                {
                                    case "E":
                                        status = OrderStatusEnum.EXECUTABLE;
                                        break;
                                    case "EC":
                                        status = OrderStatusEnum.EXECUTION_COMPLETE;
                                        break;
                                }
                            }
                            else if (reader.ValueTextEquals("rfo"u8))
                            {
                                reader.Read();
                                rfo = reader.GetString();
                            }
                            else if (reader.ValueTextEquals("rfs"u8))
                            {
                                reader.Read();
                                rfs = reader.GetString();
                            }
                            else if (reader.ValueTextEquals("cd"u8))
                            {
                                reader.Read();
                                cancelledDate = convertUnixToDateTime(reader.GetInt64());
                            }
                            else if (reader.ValueTextEquals("md"u8))
                            {
                                reader.Read();
                                matchedDate = convertUnixToDateTime(reader.GetInt64());
                            }
                            else if (reader.ValueTextEquals("pd"u8))
                            {
                                reader.Read();
                                placedDate = convertUnixToDateTime(reader.GetInt64());
                            }
                            else
                            {
                                reader.Read();
                                reader.Skip();
                            }

                        }
                    }



                    var order = new Order();
                    order.BetId = betId;
                    order.Price = p;
                    order.SizeRemaining = sr;
                    order.SizeMatched = sm;
                    order.SizeVoided = sv;
                    order.SizeLapsed = sl;
                    order.SizeCancelled = sc;
                    order.AveragePriceMatched = avp;
                    order.Side = side;
                    order.Size = s;
                    order.OrderStatus = status;
                    order.Persistence = pt;
                    order.MatchedDate = matchedDate;
                    order.CancelledDate = cancelledDate;
                    order.PlacedDate = placedDate;
                    order.OrderType = ot;
                    order.ReferenceOrder = rfo;
                    order.ReferenceStrategy = rfs;


                    runnerCache.AddOrder(order);
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
            if (statusCode == "SUCCESS")
            {
                _logger.Log("Connected.");
            }
            else
            {
                _logger.Log($"[Betfair Stream Error] Connection ID: {connectionId}");
                _logger.Log($"Status Code: {statusCode}");
                _logger.Log($"Error Code: {errorCode}");
                _logger.Log($"Details: {errorMessage}");
            }
            

            // Trigger your recovery systems or stop the socket connection loop here if unauthorized
        }


        
        
    }
}
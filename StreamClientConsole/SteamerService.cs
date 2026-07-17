using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using BetfairStreamClient.Betting;
using BetfairStreamClient.Logging;
using BetfairStreamClient.Stream;

namespace StreamClientConsole{

    public class SteamerService : IDisposable
    {
        private readonly StreamClient _streamClient;

        private readonly BetfairAsyncClient _bettingClient;

        private readonly Logger _logger;

        private readonly CancellationToken _cancellationToken;
        private readonly SteamerDetector _detector = new();

        private List<MarketCatalogue> _marketCatalogues = new List<MarketCatalogue>();
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<long, RunnerTracker>> _markets = new();

        private readonly OrderBookManager _orderBookManager = new OrderBookManager();

        
        
    
        private readonly double _stakePerBet;

        private static int _betCount=0;

        private readonly PendingLayMonitor _pendingLayMonitor;

        public SteamerService(BetfairAsyncClient bettingClient, StreamClient streamClient, Logger logger, CancellationToken cancellationToken, double stakePerBet = 1.0)
        {
            _bettingClient = bettingClient ?? throw new ArgumentNullException();
            _streamClient = streamClient ?? throw new ArgumentNullException();
            _streamClient.MarketCacheManager.MarketNotificationReceived += OnMarketPriceUpdate;
            _stakePerBet = stakePerBet;
            _logger = logger ?? throw new ArgumentNullException();
            _cancellationToken = cancellationToken;
            _pendingLayMonitor = new PendingLayMonitor(bettingClient, _logger);
            
        }

        public async Task Start()
        {
            _logger.Log("[STREAMER] Start...");
            try{
                //subscribeto race markets
                _marketCatalogues = await _bettingClient.ListMarketCatalogueAsync(new MarketFilter
                {
                    EventTypeIds = new List<string> { "7" },
                    MarketCountries= new List<string> { "AU", "NZ" },
                    MarketTypeCodes = new List<string> { "WIN" },
                    MarketBettingTypes = new List<string>{"ODDS"},
                    TurnInPlayEnabled = true,
                    InPlayOnly = false,
                    MarketStartTime = new TimeRange
                    {
                        From = DateTime.UtcNow.AddMinutes(10),
                        To = DateTime.UtcNow.AddHours(3)
                    }
                }, new List<MarketProjection>
                {
                    MarketProjection.COMPETITION, MarketProjection.RUNNER_METADATA, MarketProjection.EVENT
                });
                List<string> marketIds =  _marketCatalogues.Select(x=>x.MarketId).Take(5).ToList();
                
                if(marketIds.Count>0)
                   await _streamClient.ChangeMarketsAsync(marketIds,_cancellationToken);
                else
                   _logger.Log($"No markets available in time period.");
                await _streamClient.ChangeOrdersAsync("STRACE",_cancellationToken);
                // List<string> tempMarketIds = new List<string>{"1.260095692"};
                // await _streamClient.ChangeMarketsAsync(tempMarketIds,_cancellationToken);
                
            }
            catch(Exception ex)
            {
                _logger.Log($"[STREAMER] Exception in Start() {ex.Message}");
            }
            
            
        }

        public void OnMarketPriceUpdate(object ? sender, MarketChangeNotification market)
        {
            using(market)    //cleanly returns arrays to the pool at the end of the block
            {
                string marketId = market.MarketId;
                DateTime scheduledOff = DateTime.MinValue;
                if (market.MarketDefinition != null)
                {
                    scheduledOff = market.MarketDefinition.MarketTime ?? DateTime.UtcNow.AddHours(1);
                }
                var trackers = _markets.GetOrAdd(marketId, _ => new ConcurrentDictionary<long, RunnerTracker>());
                var cat = _marketCatalogues.FirstOrDefault(x=>x.MarketId==market.MarketId);
                string competitionName="gigi";
                string eventName = "gigi";
                if (cat != null)
                {
                    if(cat.Competition!=null)
                        competitionName = cat.Competition.Name;
                    if (cat.Event != null)
                    {
                        eventName = cat.Event.Name;
                    }
                }
                for(int i = 0; i < market.PriceSnapshot.RunnerCount; i++)
                {
                    var runnerSnap = market.PriceSnapshot.Runners[i];
                    var bestBack = SnapshotPriceExtensions.FindBestPrice(runnerSnap.BestAvailableToBack,runnerSnap.BatbCount);
                    var bestLay = SnapshotPriceExtensions.FindBestPrice(runnerSnap.BestAvailableToLay,runnerSnap.BatlCount);
                    var bestBackPrice = 0.0;
                    var bestBackSize = 0.0;
                    if (bestBack.HasValue)
                    {
                        bestBackPrice = bestBack.Value.Price;
                        bestBackSize = bestBack.Value.Size;
                    }
                    var bestLayPrice = 0.0;
                    var bestLaySize = 0.0;
                    if (bestLay.HasValue)
                    {
                        bestLayPrice = bestLay.Value.Price;
                        bestLaySize = bestLay.Value.Size;
                    }
                    if (bestBackPrice <= 1.01) continue;
                    
                    var tracker = trackers.GetOrAdd(runnerSnap.SelectionId, id => new RunnerTracker(id));
                    if(bestLayPrice <= 1.0)
                    {
                        bestLayPrice = Math.Round(Math.Max(1.01,tracker.CurrentPrice - 1.0),2);
                    }
                    if (tracker.AlreadyBacked && bestLayPrice > 1.0)
                    {

                        _ = _pendingLayMonitor.CheckAsync(marketId, runnerSnap.SelectionId, bestLayPrice);
                    }
                    else
                    {
                        if(scheduledOff!=DateTime.MinValue)
                            tracker.ScheduledOff = scheduledOff;
                        tracker.Update(bestBackPrice);

                        var score = tracker.CalculateSteamScore();
                        if (score == null) continue;

                        double wom = SteamerDetector.WeightOfMoney(bestBackSize, bestLaySize);
                        
                        if (_detector.IsSteaming(tracker, score, wom, tracker.ScheduledOff))// scheduledOff))
                        {
                            if (runnerSnap.SelectionId == 0)
                            {
                                int x= 0;
                            }
                            string runnerName = runnerSnap.SelectionId.ToString();
                            if(cat!=null)
                            {
                                if(cat.Runners!=null)
                                {                                
                                    var runnerDef = cat.Runners.FirstOrDefault(x=>x.SelectionId == runnerSnap.SelectionId);
                                    if (runnerDef != null)
                                    {
                                        runnerName = runnerDef.RunnerName;                                    
                                    }
                                }
                            }

                            _logger.Log(
                                $"STEAM DETECTED — Market: {marketId} ({eventName}) Runner: {runnerName} " +
                                $"Price: {score.CurrentPrice:F2} Drop: {score.PctDrop:F1}% " +
                                $"Vel: {score.Velocity:F3}/s WoM: {wom:P0}");

                            //tracker.AlreadyBacked = true;
                            _ = PlaceFillOrKillBackBetAsync(marketId, runnerSnap.SelectionId,score.CurrentPrice, _stakePerBet, 2, tracker);
                            //_ = ExecuteTradeAsync(marketId, runnerSnap.SelectionId, score.CurrentPrice);
                        }
                    }
                }
                
            }
        }
        public void OnOrderSnapUpdated(object? sender, OrderMarketSnap snap)
        {
            string marketId = snap.MarketId;
            for(int i = 0; i < snap.OrderCount; i++)
            {
                var order = snap.Orders[i];
                
                _orderBookManager.UpdateOrderStatus(order.BetId.ToString(), order.Status);
            }
        }
        

        public async Task PlaceFillOrKillBackBetAsync(string marketId, long selectionId, double price, double size, int killDelaySeconds, RunnerTracker tracker)
        {
            string betRef = $"STREN-{_betCount++}";
            var limitOrder = new LimitOrder(_stakePerBet, price,PersistenceType.LAPSE);
            var placeInstruction = new PlaceInstruction(selectionId,Side.BACK,limitOrder,OrderType.LIMIT,betRef);
            var placeInstructions = new List<PlaceInstruction>{ placeInstruction };
            PlaceExecutionReport placeReport;
            try{
                placeReport = await _bettingClient.PlaceOrdersAsync(marketId,        placeInstructions,"STRACE",betRef);
                tracker.AlreadyBacked=true;
                
            }
            catch(Exception ex)
            {
                _logger.Log($"[PLACEFILLORKILL] Error placing bet on market {marketId}, runner: {selectionId}, price: {price}, stake: {_stakePerBet} - {ex.Message}");
                return;
            }
            // 1. Place back bet
            string runnerName = selectionId.ToString();
            var cat = _marketCatalogues.FirstOrDefault(x=>x.MarketId == marketId);
            if(cat != null && cat.Runners != null)
            {
                var runner = cat.Runners.FirstOrDefault(x=>x.SelectionId==selectionId);
                if(runner != null)
                    runnerName = runner.RunnerName;

            }
            if (placeReport.Status == "SUCCESS" && placeReport.InstructionReports!=null)
            {
                string betId = placeReport.InstructionReports[0].BetId;

                // 3. Start the asynchronous "Kill" clock without blocking your main thread
                await Task.Delay(TimeSpan.FromSeconds(killDelaySeconds));

                BetfairStreamClient.Stream.OrderStatus currentStatus = _orderBookManager.GetOrderStatus(betId);
                if(currentStatus == BetfairStreamClient.Stream.OrderStatus.Unmatched)
                {
                    // 4. Fire the cancel command. 
                    // If it's already 100% matched, Betfair API will safely return a 'BET_TAKEN_OR_LAPSED' error, which you can ignore.
                    var cancelInstruction = new CancelInstruction(betId);
                    // var cancelInstruction = new CancelInstruction {
                    //     BetId = betId
                    //     // Leaving 'SizeReduction' null cancels the entire remaining unmatched volume
                    // };
                    List<CancelInstruction> cancelInstructions = new List<CancelInstruction>{cancelInstruction};
                    string cancelRef = $"STREX-{_betCount++}";
                    try{
                        var cancelReport = await _bettingClient.CancelOrdersAsync(marketId, cancelInstructions,"STRACE",_cancellationToken);
                        if (cancelReport.Status == "SUCCESS")
                        {
                            _logger.Log($"F&K Triggered: Unmatched portion of Bet {betId} was successfully killed.");
                            tracker.AlreadyBacked=false;
                        }
                        else
                        {
                            _logger.Log($"F&K unable to cancel bet: {cancelReport.Status}");
                            tracker.AlreadyBacked=true;
                        }
                    }
                    catch(Exception ex)
                    {
                        _logger.Log($"[PLACEFILLORKILL] Exception placing kill order for market: {marketId}, runner: {selectionId} - {ex.Message}");
                    }
                }
                _orderBookManager.RemoveOrder(betId);
                if(tracker.AlreadyBacked)
                {
                    double layTarget = TradeCalculator.LayTarget(price);
                    _logger.Log($"Backed  {runnerName} at {price}. Watching for lay at {layTarget}...");

                    _pendingLayMonitor.AddPending(marketId, selectionId, price, size);
                }
                
                
            }
            
            
            
        }
        private async Task ExecuteTradeAsync(string marketId, long runnerId, double currentPrice)
        {
            string betRef = $"STREN-{_betCount++}";
            var limitOrder = new LimitOrder(_stakePerBet, currentPrice,PersistenceType.LAPSE);
            var placeInstruction = new PlaceInstruction(runnerId,Side.BACK,limitOrder,OrderType.LIMIT,betRef);
            var placeInstructions = new List<PlaceInstruction>{ placeInstruction };
            var backResult = await _bettingClient.PlaceOrdersAsync(marketId,        placeInstructions,"STRACE",betRef);
            // 1. Place back bet
            string runnerName = runnerId.ToString();
            var cat = _marketCatalogues.FirstOrDefault(x=>x.MarketId == marketId);
            if(cat != null && cat.Runners != null)
                {
                    var runner = cat.Runners.FirstOrDefault(x=>x.SelectionId==runnerId);
                    if(runner != null)
                        runnerName = runner.RunnerName;

                }

            if (backResult.Status != "SUCCESS")
            {
                
                _logger.Log($"Unable to place back bet on runner {runnerName}");
                return;
            }

            double layTarget = TradeCalculator.LayTarget(currentPrice);
            _logger.Log($"Backed  {runnerName} at {currentPrice}. Watching for lay at {layTarget}...");

            _pendingLayMonitor.AddPending(marketId, runnerId, currentPrice, _stakePerBet);

            
        }

        public void Dispose(){}
    }

}
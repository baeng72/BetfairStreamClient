using System.Collections.Concurrent;
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
   
    private readonly double _stakePerBet;

    private static int _betCount=0;

    private readonly PendingLayMonitor _pendingLayMonitor;

    public SteamerService(BetfairAsyncClient bettingClient, StreamClient streamClient, Logger logger, CancellationToken cancellationToken, double stakePerBet = 1.0)
    {
        _bettingClient = bettingClient ?? throw new ArgumentNullException();
        _streamClient = streamClient ?? throw new ArgumentNullException();
        _stakePerBet = stakePerBet;
        _logger = logger ?? throw new ArgumentNullException();
        _cancellationToken = cancellationToken;
        _pendingLayMonitor = new PendingLayMonitor(bettingClient, _logger);
        _streamClient.MarketMessageReceived += OnMarketMessageReceived;
        //_streamClient.OrderMessageReceived += OnOrderMessageReceived;
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
            TurnInPlayEnabled = true,
            InPlayOnly = false,
            MarketStartTime = new TimeRange
            {
                From = DateTime.UtcNow.AddMinutes(10),
                To = DateTime.UtcNow.AddHours(1)
            }
        });
        List<string> marketIds =  _marketCatalogues.Select(x=>x.MarketId).Take(1).ToList();
        await _streamClient.ChangeMarketsAsync(marketIds,_cancellationToken);
        }
        catch(Exception ex)
            {
                _logger.Log($"[STREAMER] Exception in Start() {ex.Message}");
            }
        
        
    }
        public void OnMarketMessageReceived(object? sender, MarketChangeMessage msg)
        {
            if(msg.MarketChanges==null)return;
            foreach(var market in msg.MarketChanges){
            // Only act on suspended=false, status=OPEN markets
                string marketId = market.MarketId;
                DateTime scheduledOff = DateTime.MinValue;//DateTime.UtcNow.AddHours(1);
                if(market.MarketDefinition!=null){
                    if (market.MarketDefinition.Status != MarketDefinition.StatusEnum.Open) return;

                    scheduledOff = market.MarketDefinition.MarketTime ?? DateTime.UtcNow.AddHours(1);
                    
                }
                var trackers = _markets.GetOrAdd(marketId, _ => new ConcurrentDictionary<long, RunnerTracker>());

                foreach (var runner in market.RunnerChanges)
                {
                    if(runner == null) continue;

                    double bestBack = runner.BestAvailableToBack?.FirstOrDefault()[1] ?? 0;
                    double bestBackSize = runner.BestAvailableToBack?.FirstOrDefault()[2] ?? 0;
                    double bestLay = runner.BestAvailableToLay?.FirstOrDefault()[1] ?? 0;
                    double bestLaySize  = runner.BestAvailableToLay?.FirstOrDefault()[2] ?? 0;

                    if (bestBack <= 1.01) continue;

                    var tracker = trackers.GetOrAdd(runner.SelectionId, id => new RunnerTracker(id));
                    if (tracker.AlreadyBacked)
                    {
                        _ = _pendingLayMonitor.CheckAsync(marketId, runner.SelectionId, bestLay);
                    }
                    else{
                        if(scheduledOff!=DateTime.MinValue)
                            tracker.ScheduledOff = scheduledOff;
                    tracker.Update(bestBack);

                    var score = tracker.CalculateSteamScore();
                    if (score == null) continue;

                    double wom = SteamerDetector.WeightOfMoney(bestBackSize, bestLaySize);
                    
                    if (_detector.IsSteaming(tracker, score, wom, tracker.ScheduledOff))// scheduledOff))
                    {
                        string runnerName = runner.SelectionId.ToString();
                        string competition = "gigi";
                        var cat = _marketCatalogues.FirstOrDefault(x=>x.MarketId==market.MarketId);
                        if (cat != null && cat.Runners!=null)
                        {
                            if(cat.Competition!=null)
                                competition = cat.Competition.Name;
                            var runnerDef = cat.Runners.FirstOrDefault(x=>x.SelectionId == runner.SelectionId);
                            if (runnerDef != null)
                            {
                                runnerName = runnerDef.RunnerName;
                                
                            }
                        }

                        _logger.Log(
                            $"STEAM DETECTED — Market: {marketId} ({competition}) Runner: {runnerName} " +
                            $"Price: {score.CurrentPrice:F2} Drop: {score.PctDrop:F1}% " +
                            $"Vel: {score.Velocity:F3}/s WoM: {wom:P0}");

                        tracker.AlreadyBacked = true;
                        _ = ExecuteTradeAsync(marketId, runner.SelectionId, score.CurrentPrice);
                    }
                    
                }
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
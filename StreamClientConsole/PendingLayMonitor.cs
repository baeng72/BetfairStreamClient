using System.Collections.Concurrent;
using System.Drawing;
using BetfairStreamClient.Betting;
using BetfairStreamClient.Logging;
namespace StreamClientConsole{
    public class PendingLay
    {
        public double BackPrice;
        public double BackStake;
        public double LayTarget;

        public bool layPlaced;
    }
    public class PendingLayMonitor
    {
        private record MarketSelection(string marketId, long RunnerId);
        //private record PendingLay(string MarketId, long RunnerId, double BackPrice, double BackStake, double LayTarget, bool layPlaced);

        private readonly ConcurrentDictionary<MarketSelection,PendingLay> _pending = new();
        //private readonly ConcurrentBag<PendingLay> _pending = new();
        private readonly BettingClient _api;

        private static int _layCount=0;
        private readonly Logger _logger;
        public PendingLayMonitor(BettingClient api, Logger logger)
            {
                _api = api;
                _logger = logger;
            }

        public void AddPending(string marketId, long runnerId, double backPrice, double backStake)
        {
            var key = new MarketSelection(marketId, runnerId);
            double target = TradeCalculator.LayTarget(backPrice);
            if(!_pending.ContainsKey(key))
                _pending[key] = new PendingLay{
                    BackPrice = backPrice,
                    BackStake = backStake,
                    LayTarget = target,
                    layPlaced=false,
                      
                };
            
        }

        public async Task CheckAsync(string marketId, long runnerId, double currentBestLay)
        {
            var key = new MarketSelection(marketId, runnerId);
            if(_pending.TryGetValue(key, out var pendingLay))
            {
                if(pendingLay.layPlaced)return;//already been here
                if(currentBestLay < pendingLay.LayTarget)
                {
                     var (layStake, ifWins, ifLoses) = TradeCalculator.LockInProfit(
                            pendingLay.BackPrice, pendingLay.BackStake, currentBestLay);
                    _logger.Log(
                $"LAY OPPORTUNITY — stake {layStake:F2} @ {currentBestLay}. " +
                $"P&L: +{ifWins:F2} (win) / +{ifLoses:F2} (lose)");
            var limitOrder = new LimitOrder(layStake, currentBestLay, PersistenceType.LAPSE);
            string stratRef = $"LM-{_layCount++}";
            var PlaceInstruction  = new PlaceInstruction(runnerId, Side.LAY, limitOrder, OrderType.LIMIT, stratRef);

            try{
                PlaceExecutionReport report = await _api.PlaceOrdersAsync(marketId, new List<PlaceInstruction>
                {
                    PlaceInstruction
                    
                }, "LM", stratRef);
                pendingLay.layPlaced=true;
            }
            catch(Exception ex)
            {
                _logger.Log($"[PENDINGLAY] Exception: {ex.Message}, Market: {marketId}, Selection: {runnerId}, layStake {layStake}, layPrice: {currentBestLay}");
            }


                }
            }
            

        }
    }
}
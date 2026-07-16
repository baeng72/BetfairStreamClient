using System.Collections.Concurrent;
using System.Drawing;
using BetfairStreamClient.Betting;
using BetfairStreamClient.Logging;
namespace StreamClientConsole{
public class PendingLayMonitor
{
    private record PendingLay(string MarketId, long RunnerId, double BackPrice, double BackStake, double LayTarget);

    private readonly ConcurrentBag<PendingLay> _pending = new();
    private readonly BetfairAsyncClient _api;

    private static int _layCount=0;
    private readonly Logger _logger;
    public PendingLayMonitor(BetfairAsyncClient api, Logger logger)
        {
            _api = api;
            _logger = logger;
        }

    public void AddPending(string marketId, long runnerId, double backPrice, double backStake)
    {
        double target = TradeCalculator.LayTarget(backPrice);
        _pending.Add(new PendingLay(marketId, runnerId, backPrice, backStake, target));
    }

    public async Task CheckAsync(string marketId, long runnerId, double currentBestLay)
    {
        var match = _pending.FirstOrDefault(p =>
            p.MarketId == marketId &&
            p.RunnerId == runnerId &&
            currentBestLay <= p.LayTarget);

        if (match == null) return;

        var (layStake, ifWins, ifLoses) = TradeCalculator.LockInProfit(
            match.BackPrice, match.BackStake, currentBestLay);

        _logger.Log(
            $"LAY OPPORTUNITY — stake {layStake:F2} @ {currentBestLay}. " +
            $"P&L: +{ifWins:F2} (win) / +{ifLoses:F2} (lose)");
        var limitOrder = new LimitOrder(layStake, currentBestLay, PersistenceType.LAPSE);
        string stratRef = $"LM-{_layCount++}";
        var PlaceInstruction  = new PlaceInstruction(runnerId, Side.LAY, limitOrder, OrderType.LIMIT, stratRef);


        await _api.PlaceOrdersAsync(marketId, new List<PlaceInstruction>
        {
            PlaceInstruction
            
        }, "LM", stratRef);
    }
}
}
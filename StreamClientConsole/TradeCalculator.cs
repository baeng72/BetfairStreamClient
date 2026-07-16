
namespace StreamClientConsole{
public static class TradeCalculator


{
    private const double Commission = 0.05;

    /// <summary>Lay price needed to lock in targetProfitPct after commission</summary>
    public static double LayTarget(double backPrice, double targetProfitPct = 0.05)
        => Math.Round(backPrice * (1 - targetProfitPct / (1 - Commission)), 2);

    /// <summary>Matched lay stake and guaranteed P&L both outcomes</summary>
    public static (double LayStake, double ProfitIfWins, double ProfitIfLoses) LockInProfit(
        double backPrice, double backStake, double layPrice)
    {
        double layStake = Math.Round((backStake * backPrice) / layPrice, 2);
        double ifWins  = Math.Round((backPrice - 1) * backStake - (layPrice - 1) * layStake, 2);
        double ifLoses = Math.Round(layStake - backStake, 2);
        return (layStake, ifWins, ifLoses);
    }
}

}
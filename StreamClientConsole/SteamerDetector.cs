
namespace StreamClientConsole{
public class SteamerDetector
{
    // Tune these per market type
    public double MinPctDrop { get; set; } = 15.0;
    public double MinVelocity { get; set; } = 0.05;   // % drop per second
    public double MinWom { get; set; } = 0.60;         // weight of money on back side
    public double MinStartingPrice { get; set; } = 4.0;
    public int MaxSecondsToOff { get; set; } = 600;

    public bool IsSteaming(
        RunnerTracker tracker,
        SteamScore score,
        double wom,
        DateTime scheduledOff)
    {
        if (tracker.AlreadyBacked) return false;
        if (score.OldestPrice < MinStartingPrice) return false;

        double secsToOff = (scheduledOff - DateTime.UtcNow).TotalSeconds;
        if (secsToOff > MaxSecondsToOff || secsToOff < 60) return false;

        return score.PctDrop >= MinPctDrop
            && score.Velocity >= MinVelocity
            && wom >= MinWom;
    }

    /// <summary>Weight of money: proportion of money on the back side at best price</summary>
    public static double WeightOfMoney(double bestBackSize, double bestLaySize)
    {
        double total = bestBackSize + bestLaySize;
        return total == 0 ? 0.5 : bestBackSize / total;
    }
}

}
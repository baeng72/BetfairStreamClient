using System.Collections.Concurrent;
namespace StreamClientConsole{
public class RunnerTracker
{
    private readonly int _windowSeconds;
    private readonly Queue<(DateTime Timestamp, double Price)> _prices = new();

    public long RunnerId { get; }
    public double CurrentPrice { get; private set; }
    public bool AlreadyBacked { get; set; }

    public DateTime ScheduledOff{get;set;} =DateTime.UtcNow;

    public RunnerTracker(long runnerId, int windowSeconds = 60)
    {
        RunnerId = runnerId;
        _windowSeconds = windowSeconds;
    }

    public void Update(double bestBackPrice)
    {
        var now = DateTime.UtcNow;
        _prices.Enqueue((now, bestBackPrice));
        CurrentPrice = bestBackPrice;

        // Trim old entries outside the window
        while (_prices.Count > 0 && (now - _prices.Peek().Timestamp).TotalSeconds > _windowSeconds)
            _prices.Dequeue();
    }

    public SteamScore? CalculateSteamScore()
    {
        if (_prices.Count < 3) return null;

        var list = _prices.ToArray();
        double oldest = list[0].Price;
        double latest = list[^1].Price;
        double elapsedSeconds = (list[^1].Timestamp - list[0].Timestamp).TotalSeconds;

        if (elapsedSeconds < 1) return null;

        double pctDrop = (oldest - latest) / oldest * 100.0;
        double velocity = pctDrop / elapsedSeconds;

        // Acceleration: compare first vs second half velocity
        int mid = list.Length / 2;
        double v1 = (list[0].Price - list[mid].Price) /
                    Math.Max((list[mid].Timestamp - list[0].Timestamp).TotalSeconds, 0.001);
        double v2 = (list[mid].Price - list[^1].Price) /
                    Math.Max((list[^1].Timestamp - list[mid].Timestamp).TotalSeconds, 0.001);

        return new SteamScore
        {
            PctDrop = pctDrop,
            Velocity = velocity,
            Acceleration = v2 - v1,
            CurrentPrice = latest,
            OldestPrice = oldest
        };
    }
}

public record SteamScore
{
    public double PctDrop { get; init; }
    public double Velocity { get; init; }
    public double Acceleration { get; init; }
    public double CurrentPrice { get; init; }
    public double OldestPrice { get; init; }
}}
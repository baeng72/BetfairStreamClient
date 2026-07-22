using BetfairStreamClient.Stream;
using System.Buffers;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace BetfairStreamClient.Stream
{


    // Flat value types for prices and order books
    public readonly struct PriceSize
    {
        public double Price { get; }
        public double Size { get; }
        public PriceSize(double price, double size) { Price = price; Size = size; }
    }

    public enum BetfairLadderType
    {
        Bdatb = 0, Bdatl = 1, Batb = 2, Batl = 3, Atb = 4, Atl = 5, Trd = 6
    }

    //public enum OrderSide { Back, Lay }
    //public enum OrderStatus { Unmatched, Matched, ExecutionComplete }
    

    public readonly struct OrderSnap
    {
        public long BetId { get; }
        public double Price { get; }
        public double SizeRemaining { get; }
        public double SizeMatched { get; }
        public SideEnum Side { get; }
        public StatusEnum Status { get; }

        public OrderSnap(long betId, double price, double sizeRemaining, double sizeMatched, SideEnum side, StatusEnum status)
        {
            BetId = betId; Price = price; SizeRemaining = sizeRemaining; SizeMatched = sizeMatched; Side = side; Status = status;
        }
    }

    
    

    // Isolated point-in-time price snapshot
    public readonly struct MarketSnap : IDisposable
    {
        public string MarketId { get; init; }
        public DateTime Timestamp { get; init; }
        public RunnerSnap[] Runners { get; init; }
        public int RunnerCount { get; init; }

        public void Dispose()
        {
            if (Runners != null)
            {
                for (int i = 0; i < RunnerCount; i++) Runners[i].Dispose();
                ArrayPool<RunnerSnap>.Shared.Return(Runners);
            }
        }
    }

    public readonly struct RunnerSnap : IDisposable
    {
        public long SelectionId { get; init; }
        public PriceSize[] BestDisplayAvailableToBack { get; init; }
        public int BdatbCount { get; init; }
        public PriceSize[] BestDisplayAvailableToLay { get; init; }
        public int BdatlCount { get; init; }
        public PriceSize[] BestAvailableToBack { get; init; }
        public int BatbCount { get; init; }
        public PriceSize[] BestAvailableToLay { get; init; }
        public int BatlCount { get; init; }

        public PriceSize[] Traded { get; init; }

        public int TradedCount { get; init; }   

        public double LastTradedPrice { get; init; }

        public double TotalVolume { get; init; }

        public void Dispose()
        {
            if (BestDisplayAvailableToBack != null) ArrayPool<PriceSize>.Shared.Return(BestDisplayAvailableToBack);
            if (BestDisplayAvailableToLay != null) ArrayPool<PriceSize>.Shared.Return(BestDisplayAvailableToLay);
            if (BestAvailableToBack != null) ArrayPool<PriceSize>.Shared.Return(BestAvailableToBack);
            if (BestAvailableToLay != null) ArrayPool<PriceSize>.Shared.Return(BestAvailableToLay);
            if (Traded != null) ArrayPool<PriceSize>.Shared.Return(Traded);
        }
    }

    // Isolated point-in-time order snapshot
    public readonly struct OrderMarketSnap : IDisposable
    {
        public string MarketId { get; init; }
        public OrderSnap[] Orders { get; init; }
        public int OrderCount { get; init; }

        public void Dispose()
        {
            if (Orders != null) ArrayPool<OrderSnap>.Shared.Return(Orders);
        }
    }

    // Unified decoupled callback notification packaging price & definition variations
    public class MarketChangeNotification : IDisposable
    {
        public string MarketId { get; init; } = string.Empty;
        public DateTime ArrivalTime { get; init; } = DateTime.UtcNow;
        public MarketDefinition? MarketDefinition { get; init; }
        public MarketSnap PriceSnapshot { get; init; }

        public void Dispose() => PriceSnapshot.Dispose();
    }

    
}


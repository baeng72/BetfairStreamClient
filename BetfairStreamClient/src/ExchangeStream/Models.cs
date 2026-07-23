
using System.Buffers;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace BetfairStreamClient.ExchangeStream
{


    

    public enum BetfairLadderType
    {
        Bdatb = 0, Bdatl = 1, Batb = 2, Batl = 3
    }

    //public enum OrderSide { Back, Lay }
    //public enum OrderStatus { Unmatched, Matched, ExecutionComplete }
    

    public readonly struct OrderSnap
    {
        public long BetId { get; }              //the id of the order
        public double Price { get; }            //the original placed price of the order
        public double Size { get; }             //the original placed size of the order

        public double BSPLiability { get; }     //the BSP Liability of the order

        public SideEnum Side { get; }           //the side of the order

        public StatusEnum Status { get; }       //the status of the order (E = EXECUTABLE, EC = EXECUTION_COMPLETE)

        public PtEnum PertistenceType { get; }  //whether the order will persist at in play or not (L = LAPSE, P = PERSIST, MOC = Market On Close)

        public OtEnum OrderType { get; }
        public double SizeRemaining { get; }
        public double SizeMatched { get; }
        
        

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

        //public PriceSize[] Traded { get; init; }

        //public int TradedCount { get; init; }   

        public double LastTradedPrice { get; init; }

        public double TotalVolume { get; init; }

        public void Dispose()
        {
            if (BestDisplayAvailableToBack != null) ArrayPool<PriceSize>.Shared.Return(BestDisplayAvailableToBack);
            if (BestDisplayAvailableToLay != null) ArrayPool<PriceSize>.Shared.Return(BestDisplayAvailableToLay);
            if (BestAvailableToBack != null) ArrayPool<PriceSize>.Shared.Return(BestAvailableToBack);
            if (BestAvailableToLay != null) ArrayPool<PriceSize>.Shared.Return(BestAvailableToLay);
            //if (Traded != null) ArrayPool<PriceSize>.Shared.Return(Traded);
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


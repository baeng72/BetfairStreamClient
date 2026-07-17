using System.Buffers;

namespace BetfairStreamClient.Stream
{

    // Isolated runner data container populated via ArrayPool
    public readonly struct MarketRunnerSnap : IDisposable
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

        public void Dispose()
        {
            if (BestDisplayAvailableToBack != null) ArrayPool<PriceSize>.Shared.Return(BestDisplayAvailableToBack);
            if (BestDisplayAvailableToLay != null) ArrayPool<PriceSize>.Shared.Return(BestDisplayAvailableToLay);
            if (BestAvailableToBack != null) ArrayPool<PriceSize>.Shared.Return(BestAvailableToBack);
            if (BestAvailableToLay != null) ArrayPool<PriceSize>.Shared.Return(BestAvailableToLay);
        }
    }
}
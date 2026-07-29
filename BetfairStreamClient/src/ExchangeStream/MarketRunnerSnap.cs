using System.Buffers;

namespace BetfairStreamClient.ExchangeStream
{

    public struct MarketRunnerSnap<T> where T : struct, IDisposable, IClearable
    {
        public long SelectionId { get; set; }
        

        public T RunnerData;

        public void Dispose()
        {
            RunnerData.Dispose();
        }

        public void Clear() { }

    }

    public struct MarketRunnerSnapBdat : IDisposable, IClearable
    {
        public LevelPriceSize[] BestDisplayAvailableToBack;
        public LevelPriceSize[] BestDisplayAvailableToLay;
        public int BestDisplayAvailableToBackCount;
        public int BestDisplayAvailableToLayCount;
        public void Dispose()
        {
            if (BestDisplayAvailableToBack != null) ArrayPool<LevelPriceSize>.Shared.Return(BestDisplayAvailableToBack);
            if (BestDisplayAvailableToLay != null) ArrayPool<LevelPriceSize>.Shared.Return(BestDisplayAvailableToLay);
        }
        public void Clear() { }
    }

    public struct MarketRunnerSnapBat : IDisposable, IClearable
    {
        public LevelPriceSize[] BestAvailableToBack;
        public LevelPriceSize[] BestAvailableToLay;
        public int BestAvailableToBackCount;
        public int BestAvailableToLayCount;
        public void Dispose()
        {
            if (BestAvailableToBack != null) ArrayPool<LevelPriceSize>.Shared.Return(BestAvailableToBack);
            if (BestAvailableToLay != null) ArrayPool<LevelPriceSize>.Shared.Return(BestAvailableToLay);
        }
        public void Clear() { }
    }

    public struct MarketRunnerSnapAt : IDisposable, IClearable
    {
        public PriceSize[] AvailableToBack;
        public PriceSize[] AvailableToLay;
        public int AvailableToBackCount;
        public int AvailableToLayCount;
        public void Dispose()
        {
            if (AvailableToBack != null) ArrayPool<PriceSize>.Shared.Return(AvailableToBack);
            if (AvailableToLay != null) ArrayPool<PriceSize>.Shared.Return(AvailableToLay);
        }
        public void Clear() { }
    }

    public struct MarketRunnerSnapLTP : IDisposable, IClearable
    {
        public double LastTradedPrice { get; set; }
        public void Dispose() { }
        public void Clear() { }
    }

    public struct MarketRunnerSnapTV : IDisposable, IClearable
    {
        public double TradedVolume { get; set; }
        public void Dispose() { }
        public void Clear() { }
    }
    public struct MarketRunnerSnapTraded : IDisposable, IClearable
    {
        public PriceSize[] Traded;
        public int TradedCount;
        public void Dispose()
        {
            if (Traded != null) ArrayPool<PriceSize>.Shared.Return(Traded);
        }
        public void Clear() { TradedCount = 0; }
    }

    public struct MarketRunnerSnapBdatTraded : IDisposable, IClearable
    {
        public LevelPriceSize[] BestDisplayAvailableToBack;
        public LevelPriceSize[] BestDisplayAvailableToLay;
        public int BestDisplayAvailableToBackCount;
        public int BestDisplayAvailableToLayCount;
        public PriceSize[] Traded;
        public int TradedCount;
        public void Dispose()
        {
            if (BestDisplayAvailableToBack != null) ArrayPool<LevelPriceSize>.Shared.Return(BestDisplayAvailableToBack);
            if (BestDisplayAvailableToLay != null) ArrayPool<LevelPriceSize>.Shared.Return(BestDisplayAvailableToLay);
            if (Traded != null) ArrayPool<PriceSize>.Shared.Return(Traded);
        }
        public void Clear()
        {
            BestDisplayAvailableToBackCount = BestDisplayAvailableToLayCount = TradedCount = 0;
        }
    }

    public struct MarketRunnerSnapBatTraded : IDisposable, IClearable
    {
        public LevelPriceSize[] BestAvailableToBack;
        public LevelPriceSize[] BestAvailableToLay;
        public int BestAvailableToBackCount;
        public int BestAvailableToLayCount;
        public PriceSize[] Traded;
        public int TradedCount;
        public void Dispose()
        {
            if (BestAvailableToBack != null) ArrayPool<LevelPriceSize>.Shared.Return(BestAvailableToBack);
            if (BestAvailableToLay != null) ArrayPool<LevelPriceSize>.Shared.Return(BestAvailableToLay);
            if (Traded != null) ArrayPool<PriceSize>.Shared.Return(Traded);
        }
        public void Clear()
        {
            BestAvailableToBackCount = BestAvailableToLayCount = TradedCount = 0;
        }
    }

    public struct MarketRunnerSnapAtTraded : IDisposable, IClearable
    {
        public PriceSize[] AvailableToBack;
        public PriceSize[] AvailableToLay;
        public int AvailableToBackCount;
        public int AvailableToLayCount;
        public PriceSize[] Traded;
        public int TradedCount;
        public void Dispose()
        {
            if (AvailableToBack != null) ArrayPool<PriceSize>.Shared.Return(AvailableToBack);
            if (AvailableToLay != null) ArrayPool<PriceSize>.Shared.Return(AvailableToLay);
            if (Traded != null) ArrayPool<PriceSize>.Shared.Return(Traded);
        }
        public void Clear()
        {
            AvailableToBackCount = AvailableToLayCount = TradedCount = 0;
        }
    }

    public struct MarketRunnerSnapBdatTVLTP : IDisposable, IClearable
    {
        public LevelPriceSize[] BestDisplayAvailableToBack;
        public LevelPriceSize[] BestDisplayAvailableToLay;
        public int BestDisplayAvailableToBackCount;
        public int BestDisplayAvailableToLayCount;
        public double TradedVolume { get; set; }
        public double LastTradedPrice { get; set; }
        public void Dispose()
        {
            if (BestDisplayAvailableToBack != null) ArrayPool<LevelPriceSize>.Shared.Return(BestDisplayAvailableToBack);
            if (BestDisplayAvailableToLay != null) ArrayPool<LevelPriceSize>.Shared.Return(BestDisplayAvailableToLay);
        }
        public void Clear()
        {
            BestDisplayAvailableToBackCount = BestDisplayAvailableToBackCount = 0;
            TradedVolume = LastTradedPrice = 0.0;
        }
    }

    public struct MarketRunnerSnapBdatTradedTVLTP : IDisposable, IClearable
    {
        public LevelPriceSize[] BestDisplayAvailableToBack;
        public LevelPriceSize[] BestDisplayAvailableToLay;
        public int BestDisplayAvailableToBackCount;
        public int BestDisplayAvailableToLayCount;
        public PriceSize[] Traded;
        public int TradedCount;
        public double TradedVolume { get; set; }
        public double LastTradedPrice { get; set; }
        public void Dispose()
        {
            if (BestDisplayAvailableToBack != null) ArrayPool<LevelPriceSize>.Shared.Return(BestDisplayAvailableToBack);
            if (BestDisplayAvailableToLay != null) ArrayPool<LevelPriceSize>.Shared.Return(BestDisplayAvailableToLay);
            if (Traded != null) ArrayPool<PriceSize>.Shared.Return(Traded);
        }
        public void Clear()
        {
            BestDisplayAvailableToBackCount = BestDisplayAvailableToLayCount = TradedCount = 0;
            TradedVolume = LastTradedPrice = 0.0;
        }
    }

    public struct MarketRunnerSnapBatTVLTP : IDisposable, IClearable
    {
        public LevelPriceSize[] BestAvailableToBack;
        public LevelPriceSize[] BestAvailableToLay;
        public int BestAvailableToBackCount;
        public int BestAvailableToLayCount;
        public double TradedVolume { get; set; }
        public double LastTradedPrice { get; set; }
        public void Dispose()
        {
            if (BestAvailableToBack != null) ArrayPool<LevelPriceSize>.Shared.Return(BestAvailableToBack);
            if (BestAvailableToLay != null) ArrayPool<LevelPriceSize>.Shared.Return(BestAvailableToLay);
        }
        public void Clear()
        {
            BestAvailableToBackCount = BestAvailableToLayCount = 0;
            TradedVolume = LastTradedPrice = 0.0;
        }
    }

    public struct MarketRunnerSnapBatTradedTVLTP : IDisposable, IClearable
    {
        public LevelPriceSize[] BestAvailableToBack;
        public LevelPriceSize[] BestAvailableToLay;
        public int BestAvailableToBackCount;
        public int BestAvailableToLayCount;
        public PriceSize[] Traded;
        public int TradedCount;
        public double TradedVolume { get; set; }
        public double LastTradedPrice { get; set; }
        public void Dispose()
        {
            if (BestAvailableToBack != null) ArrayPool<LevelPriceSize>.Shared.Return(BestAvailableToBack);
            if (BestAvailableToLay != null) ArrayPool<LevelPriceSize>.Shared.Return(BestAvailableToLay);
            if (Traded != null) ArrayPool<PriceSize>.Shared.Return(Traded);
        }
        public void Clear()
        {
            BestAvailableToBackCount = BestAvailableToLayCount = TradedCount = 0;
            LastTradedPrice = TradedVolume = 0.0;
        }
    }

    public struct MarketRunnerSnapAtTradedTVLTP : IDisposable, IClearable
    {
        public PriceSize[] AvailableToBack;
        public PriceSize[] AvailableToLay;
        public int AvailableToBackCount;
        public int AvailableToLayCount;
        public PriceSize[] Traded;
        public int TradedCount;
        public double TradedVolume { get; set; }
        public double LastTradedPrice { get; set; }
        public void Dispose()
        {
            if (AvailableToBack != null) ArrayPool<PriceSize>.Shared.Return(AvailableToBack);
            if (AvailableToLay != null) ArrayPool<PriceSize>.Shared.Return(AvailableToLay);
            if (Traded != null) ArrayPool<PriceSize>.Shared.Return(Traded);
        }
        public void Clear()
        {
            AvailableToBackCount = AvailableToLayCount = TradedCount = 0;
            LastTradedPrice = TradedVolume = 0.0;
        }
    }
}
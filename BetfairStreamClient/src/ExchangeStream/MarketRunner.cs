using BetfairStreamClient.Betting;
using BetfairStreamClient.ExchangeStream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace BetfairStreamClient.ExchangeStream
{    
    public interface IClearable
    {
        void Clear();
    }
    public class MarketRunner<T> where T : struct, IClearable, IDisposable
    {
        public long SelectionId { get; set; } = 0;

        public T RunnerData;
        public void Clear()
        {
            RunnerData.Clear();
        }
        public void Dispose() { RunnerData.Dispose(); }
    }

    public struct MarketRunnerBdat : IClearable, IDisposable
    {
        public const int MaxBdatCount = 10;

        public LevelPriceSizeCache BestDisplayAvailableToBack;
        public int BestDisplayAvailableToBackCount = 0;
        public LevelPriceSizeCache BestDisplayAvailableToLay;
        public int BestDisplayAvailableToLayCount = 0;
        public MarketRunnerBdat()
        {
        }
        public void Clear()
        {
            BestDisplayAvailableToBackCount = BestDisplayAvailableToBackCount = 0;
        }
        public void Dispose() { }
    }

    public struct MarketRunnerBat : IClearable, IDisposable
    {
        public const int MaxBatCount = 10;        
        public LevelPriceSizeCache BestAvailableToBack;
        public int BestAvailableToBackCount = 0;
        public LevelPriceSizeCache BestAvailableToLay;
        public int BestAvailableToLayCount = 0;
        public MarketRunnerBat()
        {
        }
        public void Clear()
        {
            BestAvailableToBackCount = BestAvailableToLayCount = 0;
        }
        public void Dispose() { }
    }

    public struct MarketRunnerAt : IClearable, IDisposable
    {
        public const int MaxAtCount = 20;
        public PriceSizeLadder AvailableToBack;
        public int AvailableToBackCount = 0;
        public PriceSizeLadder AvailableToLay;
        public int AvailableToLayCount = 0;
        public MarketRunnerAt()
        {
        }
        public void Clear()
        {
            AvailableToBackCount = AvailableToLayCount = 0;
        }
        public void Dispose() { }
    }

    public struct MarketRunnerTraded : IClearable, IDisposable
    {
        public const int MaxTradedCount = 20;
        public PriceSizeLadder Traded;
        public int TradedCount = 0;
        public MarketRunnerTraded()
        {
        }
        public void Clear()
        {
            TradedCount = 0;
        }
        public void Dispose() { }
    }

    public struct MarketRunnerLastTradedPrice : IClearable, IDisposable
    {
        public double LastTradedPrice { get; set; } = 0.0;
        public MarketRunnerLastTradedPrice()
        {
        }
        public void Clear()
        {
            LastTradedPrice = 0.0;
        }
        public void Dispose() { }
    }

    public struct MarketRunnerTradedVolume : IClearable, IDisposable
    {
        public double TradedVolume { get; set; } = 0.0;
        public MarketRunnerTradedVolume()
        {
        }
        public void Clear()
        {
            TradedVolume = 0.0;
        }
        public void Dispose() { }
    }

    //composite - sort of - types    
    public struct MarketRunnerAtTraded : IClearable, IDisposable
    {
        public const int MaxAtCount = 20;
        public PriceSizeLadder AvailableToBack;
        public int AvailableToBackCount;
        public PriceSizeLadder AvailableToLay;
        public int AvailableToLayCount;
        public const int MaxTradedCount = 20;
        public PriceSizeLadder Traded;
        public int TradedCount;
        public MarketRunnerAtTraded()
        {
            AvailableToBackCount = 0;
            AvailableToLayCount = 0;
            TradedCount = 0;
        }
        public void Clear()
        {
            AvailableToBackCount = AvailableToLayCount = TradedCount = 0;
        }
        public void Dispose()
        {

        }
    }

    public struct MarketRunnerAtTradedTVLTP : IClearable, IDisposable
    {
        public const int MaxAtCount = 20;
        public PriceSizeLadder AvailableToBack;
        public int AvailableToBackCount;
        public PriceSizeLadder AvailableToLay;
        public int AvailableToLayCount;
        public const int MaxTradedCount = 20;
        public PriceSizeLadder Traded;
        public int TradedCount;
        public double LastTradedPrice = 0.0;
        public double TradedVolume = 0.0;
        public MarketRunnerAtTradedTVLTP()
        {
            AvailableToBackCount = 0;
            AvailableToLayCount = 0;
            TradedCount = 0;
            LastTradedPrice = TradedVolume = 0.0;
        }
        public void Clear()
        {
            AvailableToBackCount = AvailableToLayCount = TradedCount = 0;
            LastTradedPrice = TradedVolume = 0.0;
        }
        public void Dispose()
        {

        }
    }
    public struct MarketRunnerBatTraded : IClearable, IDisposable
    {
        public const int MaxBatCount = 10;        
        public LevelPriceSizeCache BestAvailableToBack;
        public int BestAvailableToBackCount;
        public LevelPriceSizeCache BestAvailableToLay;
        public int BestAvailableToLayCount;
        public const int MaxTradedCount = 20;
        public PriceSizeLadder Traded;
        public int TradedCount;
        public MarketRunnerBatTraded()
        {
            BestAvailableToBackCount = 0;
            BestAvailableToLayCount = 0;            
            TradedCount = 0;
        }
        public void Clear()
        {
            BestAvailableToBackCount = BestAvailableToLayCount = TradedCount = 0;
        }
        public void Dispose() { }
    }
    public struct MarketRunnerBdatTraded : IClearable, IDisposable
    {
        public const int MaxBdatCount = 10;
        public LevelPriceSizeCache BestDisplayAvailableToBack;
        public int BestDisplayAvailableToBackCount;
        public LevelPriceSizeCache BestDisplayAvailableToLay;
        public int BestDisplayAvailableToLayCount;
        public const int MaxTradedCount = 20;
        public PriceSizeLadder Traded;
        public int TradedCount;
        public MarketRunnerBdatTraded()
        {
            BestDisplayAvailableToBackCount = 0;
            BestDisplayAvailableToLayCount = 0;
            TradedCount = 0;
        }
        public void Clear()
        {
            BestDisplayAvailableToBackCount = BestDisplayAvailableToLayCount = TradedCount = 0;
        }
        public void Dispose() { }
    }

    public struct MarketRunnerBatTVLTP : IClearable, IDisposable
    {
        public const int MaxBatCount = 10;        
        public LevelPriceSizeCache BestAvailableToBack;
        public int BestAvailableToBackCount;
        public LevelPriceSizeCache BestAvailableToLay;
        public int BestAvailableToLayCount;
        public double LastTradedPrice = 0.0;
        public double TradedVolume = 0.0;
        public MarketRunnerBatTVLTP()
        {
            BestAvailableToBackCount = BestAvailableToLayCount = 0;
            TradedVolume = LastTradedPrice = 0.0;
        }
        public void Clear()
        {
            BestAvailableToBackCount = BestAvailableToLayCount = 0;
            LastTradedPrice = 0.0;
            TradedVolume = 0.0;
        }
        public void Dispose() { }
    }

    public struct MarketRunnerBatTradedTVLTP : IClearable, IDisposable
    {
        public const int MaxBatCount = 10;        
        public LevelPriceSizeCache BestAvailableToBack;
        public int BestAvailableToBackCount;
        public LevelPriceSizeCache BestAvailableToLay;
        public int BestAvailableToLayCount;
        public const int MaxTradedCount = 20;
        public PriceSizeLadder Traded;
        public int TradedCount;
        public double LastTradedPrice = 0.0;
        public double TradedVolume = 0.0;
        public MarketRunnerBatTradedTVLTP()
        {
            BestAvailableToBackCount = 0;
            BestAvailableToLayCount = 0;
            TradedCount = 0;
            LastTradedPrice = 0.0;
            TradedVolume = 0.0;
        }
        public void Clear()
        {
            BestAvailableToBackCount = BestAvailableToLayCount = TradedCount = 0;
            LastTradedPrice = 0.0;
            TradedVolume = 0.0;
        }
        public void Dispose() { }
    }
}

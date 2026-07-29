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
        public LevelPriceSizeCache BestDisplayAvailableToBack;
        public int BestDisplayAvailableToBackCount { get { return BestDisplayAvailableToBack.Count; } }
        public LevelPriceSizeCache BestDisplayAvailableToLay;
        public int BestDisplayAvailableToLayCount { get { return BestDisplayAvailableToLay.Count; } }
        public MarketRunnerBdat()
        {
        }
        public void Clear()
        {            
            BestDisplayAvailableToBack.Clear();
            BestDisplayAvailableToLay.Clear();
        }
        public void Dispose() { }
    }

    public struct MarketRunnerBat : IClearable, IDisposable
    {        
        public LevelPriceSizeCache BestAvailableToBack;
        public int BestAvailableToBackCount { get { return BestAvailableToBack.Count; } }
        public LevelPriceSizeCache BestAvailableToLay;
        public int BestAvailableToLayCount { get { return BestAvailableToLay.Count; } }
        public MarketRunnerBat()
        {
        }
        public void Clear()
        {            
            BestAvailableToBack.Clear();
            BestAvailableToLay.Clear();
        }
        public void Dispose() { }
    }

    public struct MarketRunnerAt : IClearable, IDisposable
    {
        
        public PriceSizeLadder AvailableToBack;
        public int AvailableToBackCount { get {return AvailableToBack.LadderCount; } }
        public PriceSizeLadder AvailableToLay;
        public int AvailableToLayCount { get { return AvailableToLay.LadderCount; } }
        public MarketRunnerAt()
        {
        }
        public void Clear()
        {        
            AvailableToBack.Clear();
            AvailableToLay.Clear();
        }
        public void Dispose() { }
    }

    public struct MarketRunnerTraded : IClearable, IDisposable
    {
        
        public PriceSizeLadder Traded;
        public int TradedCount { get { return Traded.LadderCount; } }
        public MarketRunnerTraded()
        {
        }
        public void Clear()
        {            
            Traded.Clear();
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
        public PriceSizeLadder AvailableToBack;
        public int AvailableToBackCount { get { return AvailableToBack.LadderCount; } }
        public PriceSizeLadder AvailableToLay;
        public int AvailableToLayCount { get { return AvailableToLay.LadderCount; } }         
        public PriceSizeLadder Traded;
        public int TradedCount { get { return Traded.LadderCount; } }
        public MarketRunnerAtTraded()
        {            
        }
        public void Clear()
        {
            AvailableToBack.Clear();
            AvailableToLay.Clear();
            Traded.Clear();
            
        }
        public void Dispose()
        {

        }
    }

    public struct MarketRunnerAtTradedTVLTP : IClearable, IDisposable
    {
        
        public PriceSizeLadder AvailableToBack;
        public int AvailableToBackCount { get { return AvailableToBack.LadderCount; } }
        public PriceSizeLadder AvailableToLay;
        public int AvailableToLayCount { get { return AvailableToLay.LadderCount; } }        
        public PriceSizeLadder Traded;
        public int TradedCount { get { return Traded.LadderCount; } }
        public double LastTradedPrice = 0.0;
        public double TradedVolume = 0.0;
        public MarketRunnerAtTradedTVLTP()
        {            
            LastTradedPrice = TradedVolume = 0.0;
        }
        public void Clear()
        {
            
            LastTradedPrice = TradedVolume = 0.0;
            AvailableToBack.Clear();
            AvailableToLay.Clear();
            Traded.Clear(); 
        }
        public void Dispose()
        {

        }
    }
    public struct MarketRunnerBatTraded : IClearable, IDisposable
    {        
        public LevelPriceSizeCache BestAvailableToBack;
        public int BestAvailableToBackCount { get { return BestAvailableToBack.Count; } }
        public LevelPriceSizeCache BestAvailableToLay;
        public int BestAvailableToLayCount { get { return BestAvailableToLay.Count; } }
        
        public PriceSizeLadder Traded;
        public int TradedCount { get { return Traded.LadderCount; } }
        public MarketRunnerBatTraded()
        {            
        }
        public void Clear()
        {
            BestAvailableToBack.Clear();
            BestAvailableToLay.Clear();
            Traded.Clear();
        }
        public void Dispose() { }
    }
    public struct MarketRunnerBdatTraded : IClearable, IDisposable
    {
        
        public LevelPriceSizeCache BestDisplayAvailableToBack;
        public int BestDisplayAvailableToBackCount { get { return BestDisplayAvailableToBack.Count; } }
        public LevelPriceSizeCache BestDisplayAvailableToLay;
        public int BestDisplayAvailableToLayCount { get { return BestDisplayAvailableToLay.Count; } }
        
        public PriceSizeLadder Traded;
        public int TradedCount { get { return Traded.LadderCount; } }
        public MarketRunnerBdatTraded()
        {            
        }
        public void Clear()
        {
            BestDisplayAvailableToBack.Clear();
            BestDisplayAvailableToLay.Clear();
            Traded.Clear();
        }
        public void Dispose() { }
    }

    public struct MarketRunnerBatTVLTP : IClearable, IDisposable
    {
        
        public LevelPriceSizeCache BestAvailableToBack;
        public int BestAvailableToBackCount { get { return BestAvailableToBack.Count; } }
        public LevelPriceSizeCache BestAvailableToLay;
        public int BestAvailableToLayCount { get { return BestAvailableToLay.Count; } }
        public double LastTradedPrice = 0.0;
        public double TradedVolume = 0.0;
        public MarketRunnerBatTVLTP()
        {
            
            TradedVolume = LastTradedPrice = 0.0;
        }
        public void Clear()
        {
            
            LastTradedPrice = 0.0;
            TradedVolume = 0.0;
            BestAvailableToBack.Clear();
            BestAvailableToLay.Clear();
        }
        public void Dispose() { }
    }

    public struct MarketRunnerBatTradedTVLTP : IClearable, IDisposable
    {
        
        public LevelPriceSizeCache BestAvailableToBack;
        public int BestAvailableToBackCount { get {return BestAvailableToBack.Count; } }
        public LevelPriceSizeCache BestAvailableToLay;
        public int BestAvailableToLayCount { get { return BestAvailableToLay.Count; } }
        
        public PriceSizeLadder Traded;
        public int TradedCount { get { return Traded.LadderCount; } }
        public double LastTradedPrice = 0.0;
        public double TradedVolume = 0.0;
        public MarketRunnerBatTradedTVLTP()
        {
            LastTradedPrice = 0.0;
            TradedVolume = 0.0;
        }
        public void Clear()
        {
            LastTradedPrice = 0.0;
            TradedVolume = 0.0;
            BestAvailableToBack.Clear();
            BestAvailableToLay.Clear();
            Traded.Clear();
        }
        public void Dispose() { }
    }
}

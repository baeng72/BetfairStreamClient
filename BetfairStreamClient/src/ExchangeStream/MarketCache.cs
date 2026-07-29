using BetfairStreamClient.Betting;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BetfairStreamClient.ExchangeStream
{
    public class MarketCache<T, TSnap> where T : struct, IClearable, IDisposable where TSnap : struct, IDisposable, IClearable
    {
        public string MarketId { get; set; }
        private Dictionary<long, MarketRunner<T>> _runners = new Dictionary<long, MarketRunner<T>>();
        public Dictionary<long, MarketRunner<T>> Runners { get { return _runners; } }
        public int RunnerCount { get { return _runners.Count; } }
        public MarketCache(string marketId)
        {
            MarketId = marketId;
        }

        public void Clear()
        {
            foreach (var runner in _runners)
            {
                runner.Value.Clear();
            }
        }

        public MarketRunner<T> GetOrCreateRunner(long selectionId)
        {
            if (_runners.TryGetValue(selectionId, out var runner)) return runner;
            runner = new MarketRunner<T>();
            //if (typeof(T) == typeof(MarketRunnerBdat))
            //{
            //    var target = (MarketRunner<MarketRunnerBdat>)(object)runner;
            //    ref MarketRunnerBdat marketRunner = ref target.RunnerData;
            //    marketRunner.BestDisplayAvailableToBack = new LevelDelta[MarketRunnerBdat.MaxBdatCount];
            //    marketRunner.BestDisplayAvailableToLay = new LevelDelta[MarketRunnerBdat.MaxBdatCount];
            //    target.RunnerData = marketRunner;
            //}
            //else if (typeof(T) == typeof(MarketRunnerBat))
            //{
            //    var target = (MarketRunner<MarketRunnerBat>)(object)runner;
            //    ref MarketRunnerBat marketRunner = ref target.RunnerData;
            //    marketRunner.BestAvailableToBack = new LevelDelta[MarketRunnerBat.MaxBatCount];
            //    marketRunner.BestAvailableToLay = new LevelDelta[MarketRunnerBat.MaxBatCount];
            //    target.RunnerData = marketRunner;
            //}
            //else if (typeof(T) == typeof(MarketRunnerAt))
            //{
            //    var target = (MarketRunner<MarketRunnerAt>)(object)runner;
            //    ref MarketRunnerAt marketRunner = ref target.RunnerData;
            //    marketRunner.AvailableToBack = new PriceSizeDelta[MarketRunnerAt.MaxAtCount];
            //    marketRunner.AvailableToLay = new PriceSizeDelta[MarketRunnerAt.MaxAtCount];
            //    target.RunnerData = marketRunner;
            //}
            //else if (typeof(T) == typeof(MarketRunnerTraded))
            //{
            //    var target = (MarketRunner<MarketRunnerTraded>)(object)runner;
            //    ref MarketRunnerTraded marketRunner = ref target.RunnerData;
            //    marketRunner.Traded = new PriceSizeDelta[MarketRunnerTraded.MaxTradedCount];
            //    target.RunnerData = marketRunner;
            //}
            //else if (typeof(T) == typeof(MarketRunnerBdatTraded))
            //{
            //    var target = (MarketRunner<MarketRunnerBdatTraded>)(object)runner;
            //    ref MarketRunnerBdatTraded marketRunner = ref target.RunnerData;
            //    marketRunner.BestDisplayAvailableToBack = new LevelDelta[MarketRunnerBdat.MaxBdatCount];
            //    marketRunner.BestDisplayAvailableToLay = new LevelDelta[MarketRunnerBdat.MaxBdatCount];
            //    marketRunner.Traded = new PriceSizeDelta[MarketRunnerTraded.MaxTradedCount];                
            //}
            //else if (typeof(T) == typeof(MarketRunnerBatTraded))
            //{
            //    var target = (MarketRunner<MarketRunnerBatTraded>)(object)runner;
            //    ref MarketRunnerBatTraded marketRunner = ref target.RunnerData;
            //    marketRunner.BestAvailableToBack = new LevelDelta[MarketRunnerBat.MaxBatCount];
            //    marketRunner.BestAvailableToLay = new LevelDelta[MarketRunnerBat.MaxBatCount];
            //    marketRunner.Traded = new PriceSizeDelta[MarketRunnerTraded.MaxTradedCount];                
            //}
            //else if (typeof(T) == typeof(MarketRunnerAtTraded))
            //{
            //    var target = (MarketRunner<MarketRunnerAtTraded>)(object)runner;
            //    ref MarketRunnerAtTraded marketRunner = ref target.RunnerData;
            //    marketRunner.AvailableToBack = new PriceSizeDelta[MarketRunnerAt.MaxAtCount];
            //    marketRunner.AvailableToLay = new PriceSizeDelta[MarketRunnerAt.MaxAtCount];
            //    marketRunner.Traded = new PriceSizeDelta[MarketRunnerTraded.MaxTradedCount];                
            //}
            //else if (typeof(T) == typeof(MarketRunnerBatTradedTVLTP))
            //{
            //    var target = (MarketRunner<MarketRunnerBatTradedTVLTP>)(object)runner;
            //    ref MarketRunnerBatTradedTVLTP marketRunner = ref target.RunnerData;
            //    marketRunner.BestAvailableToBack = new LevelDelta[MarketRunnerBat.MaxBatCount];
            //    marketRunner.BestAvailableToLay = new LevelDelta[MarketRunnerBat.MaxBatCount];
            //    marketRunner.Traded = new PriceSizeDelta[MarketRunnerTraded.MaxTradedCount];                
            //}
            //else if (typeof(T) == typeof(MarketRunnerBatTVLTP))
            //{
            //    var target = (MarketRunner<MarketRunnerBatTVLTP>)(object)runner;
            //    ref MarketRunnerBatTVLTP marketRunner = ref target.RunnerData;
            //    marketRunner.BestAvailableToBack = new LevelDelta[MarketRunnerBat.MaxBatCount];
            //    marketRunner.BestAvailableToLay = new LevelDelta[MarketRunnerBat.MaxBatCount];                
            //}
            //else
            if (typeof(T) == typeof(MarketRunnerAtTradedTVLTP))
            {
                  var target = (MarketRunner<MarketRunnerAtTradedTVLTP>)(object)runner;
                
                ref MarketRunnerAtTradedTVLTP marketRunner = ref target.RunnerData;
                marketRunner.AvailableToBack.Initialize();
                marketRunner.AvailableToLay.Initialize();
                marketRunner.Traded.Initialize();
            //    marketRunner.AvailableToBack = new PriceSizeDelta[MarketRunnerAt.MaxAtCount];                
            //    marketRunner.AvailableToLay = new PriceSizeDelta[MarketRunnerAt.MaxAtCount];
            //    marketRunner.Traded = new PriceSizeDelta[MarketRunnerTraded.MaxTradedCount];                
            }
            _runners[selectionId] = runner;
            return runner;
        }

        //private LevelPriceSize[] RentAndCopy(LevelPriceSize[] levelDeltas, out int count)
        //{
        //    ReadOnlySpan<LevelPriceSize> copy = levelDeltas.Length > 0 ? levelDeltas.AsSpan(0, levelDeltas.Length) : ReadOnlySpan<LevelPriceSize>.Empty;
        //    count = levelDeltas.Length;
        //    if (count == 0) return Array.Empty<LevelPriceSize>();
        //    LevelPriceSize[] buffer = ArrayPool<LevelPriceSize>.Shared.Rent(count);
        //    copy.CopyTo(buffer);
        //    return buffer;
        //}
        private LevelPriceSize[] RentAndCopy(in LevelPriceSizeCache priceDeltas)
        {
            int count = priceDeltas.Count;
            if (count <= 0) return Array.Empty<LevelPriceSize>();
            var activeLevels = priceDeltas.ActiveLevels;
            ReadOnlySpan<LevelPriceSize> copy = activeLevels.Span;// ((ReadOnlySpan<LevelPriceSize>)priceDeltas).Slice(0, count);
            LevelPriceSize[] buffer = ArrayPool<LevelPriceSize>.Shared.Rent(activeLevels.Length);
            copy.CopyTo(buffer);
            return buffer;
        }

        
        private PriceSize[] RentAndCopy(in PriceSizeLadder priceDeltas, bool descending)
        {
            int count = priceDeltas.LadderCount;
            if(count <= 0) return Array.Empty<PriceSize>();
            Span<PriceSize> priceSizeBuffer = stackalloc PriceSize[350];
            priceDeltas.CopyToPriceSizeSpan(priceSizeBuffer, descending);
            Span<PriceSize> activeMarketPairs = priceSizeBuffer.Slice(0, count);
            
            PriceSize[] buffer = ArrayPool<PriceSize>.Shared.Rent(count);            
            activeMarketPairs.CopyTo(buffer);
            return buffer;
        }
        public MarketRunnerSnap<TSnap> ExtractPooledSnapshot(long selectionId)
        {
            MarketRunner<T> runner = _runners[selectionId];

            if (typeof(T) == typeof(MarketRunnerBdat))
            {
                var target = (MarketRunner<MarketRunnerBdat>)(object)runner;
                ref MarketRunnerBdat marketRunner = ref target.RunnerData;
                LevelPriceSize[] bdatb = RentAndCopy(in marketRunner.BestDisplayAvailableToBack);
                LevelPriceSize[] bdatl = RentAndCopy(in marketRunner.BestDisplayAvailableToLay);

                MarketRunnerSnapBdat snap = new MarketRunnerSnapBdat
                {
                    BestDisplayAvailableToBack = bdatb,
                    BestDisplayAvailableToLay = bdatl,
                    BestDisplayAvailableToBackCount = marketRunner.BestDisplayAvailableToBackCount,
                    BestDisplayAvailableToLayCount = marketRunner.BestDisplayAvailableToLayCount,
                };
                var result = new MarketRunnerSnap<MarketRunnerSnapBdat>
                {
                    SelectionId = selectionId,
                    RunnerData = snap,
                };
                return Unsafe.As<MarketRunnerSnap<MarketRunnerSnapBdat>, MarketRunnerSnap<TSnap>>(ref result);
            }
            else if (typeof(T) == typeof(MarketRunnerBat))
            {
                var target = (MarketRunner<MarketRunnerBat>)(object)runner;
                ref MarketRunnerBat marketRunner = ref target.RunnerData;                
                LevelPriceSize[] bdatb = RentAndCopy(in marketRunner.BestAvailableToBack);                
                LevelPriceSize[] bdatl = RentAndCopy(in marketRunner.BestAvailableToLay);

                MarketRunnerSnapBat snap = new MarketRunnerSnapBat
                {
                    BestAvailableToBack = bdatb,
                    BestAvailableToLay = bdatl,
                    BestAvailableToBackCount = marketRunner.BestAvailableToBackCount,
                    BestAvailableToLayCount = marketRunner.BestAvailableToLayCount,
                };
                var result = new MarketRunnerSnap<MarketRunnerSnapBat>
                {
                    SelectionId = selectionId,
                    RunnerData = snap,
                };
                return Unsafe.As<MarketRunnerSnap<MarketRunnerSnapBat>, MarketRunnerSnap<TSnap>>(ref result);
            }
            else if (typeof(T) == typeof(MarketRunnerBatTVLTP))
            {
                var target = (MarketRunner<MarketRunnerBatTVLTP>)(object)runner;
                ref MarketRunnerBatTVLTP marketRunner = ref target.RunnerData;                
                LevelPriceSize[] bdatb = RentAndCopy(in marketRunner.BestAvailableToBack);                
                LevelPriceSize[] bdatl = RentAndCopy(in marketRunner.BestAvailableToLay);

                MarketRunnerSnapBatTVLTP snap = new MarketRunnerSnapBatTVLTP
                {
                    BestAvailableToBack = bdatb,
                    BestAvailableToLay = bdatl,
                    BestAvailableToBackCount = marketRunner.BestAvailableToBackCount,
                    BestAvailableToLayCount = marketRunner.BestAvailableToLayCount,
                    TradedVolume = marketRunner.TradedVolume,
                    LastTradedPrice = marketRunner.LastTradedPrice,
                };
                var result = new MarketRunnerSnap<MarketRunnerSnapBatTVLTP>
                {
                    SelectionId = selectionId,
                    RunnerData = snap,
                };
                return Unsafe.As<MarketRunnerSnap<MarketRunnerSnapBatTVLTP>, MarketRunnerSnap<TSnap>>(ref result);
            }
            else if (typeof(T) == typeof(MarketRunnerBatTradedTVLTP))
            {
                var target = (MarketRunner<MarketRunnerBatTradedTVLTP>)(object)runner;
                ref MarketRunnerBatTradedTVLTP marketRunner = ref target.RunnerData;                
                LevelPriceSize[] bdatb = RentAndCopy(marketRunner.BestAvailableToBack);                
                LevelPriceSize[] bdatl = RentAndCopy(marketRunner.BestAvailableToLay);
                
                PriceSize[] traded = RentAndCopy(marketRunner.Traded, false);
                MarketRunnerSnapBatTradedTVLTP snap = new MarketRunnerSnapBatTradedTVLTP
                {
                    BestAvailableToBack = bdatb,
                    BestAvailableToLay = bdatl,
                    BestAvailableToBackCount = marketRunner.BestAvailableToBackCount,
                    BestAvailableToLayCount = marketRunner.BestAvailableToLayCount,
                    TradedVolume = marketRunner.TradedVolume,
                    LastTradedPrice = marketRunner.LastTradedPrice,
                    Traded = traded,
                    TradedCount = marketRunner.TradedCount
                };
                var result = new MarketRunnerSnap<MarketRunnerSnapBatTradedTVLTP>
                {
                    SelectionId = selectionId,
                    RunnerData = snap,
                };
                return Unsafe.As<MarketRunnerSnap<MarketRunnerSnapBatTradedTVLTP>, MarketRunnerSnap<TSnap>>(ref result);
            }
            else if (typeof(T) == typeof(MarketRunnerAt))
            {
                var target = (MarketRunner<MarketRunnerAt>)(object)runner;
                ref MarketRunnerAt marketRunner = ref target.RunnerData;                
                PriceSize[] atb = RentAndCopy(marketRunner.AvailableToBack, true);                
                PriceSize[] atl = RentAndCopy(marketRunner.AvailableToLay, false);


                MarketRunnerSnapAt snap = new MarketRunnerSnapAt
                {
                    AvailableToBack = atb,
                    AvailableToLay = atl,
                    AvailableToBackCount = marketRunner.AvailableToBackCount,
                    AvailableToLayCount = marketRunner.AvailableToLayCount

                };
                var result = new MarketRunnerSnap<MarketRunnerSnapAt>
                {
                    SelectionId = selectionId,
                    RunnerData = snap,
                };
                return Unsafe.As<MarketRunnerSnap<MarketRunnerSnapAt>, MarketRunnerSnap<TSnap>>(ref result);
            }
            else if (typeof(T) == typeof(MarketRunnerAtTradedTVLTP))
            {
                var target = (MarketRunner<MarketRunnerAtTradedTVLTP>)(object)runner;
                ref MarketRunnerAtTradedTVLTP marketRunner = ref target.RunnerData;                
                PriceSize[] atb = RentAndCopy(marketRunner.AvailableToBack, true);                
                PriceSize[] atl = RentAndCopy(marketRunner.AvailableToLay, false);
                PriceSize[] traded = RentAndCopy(marketRunner.Traded, false);


                MarketRunnerSnapAtTradedTVLTP snap = new MarketRunnerSnapAtTradedTVLTP
                {
                    AvailableToBack = atb,
                    AvailableToLay = atl,
                    Traded = traded,
                    AvailableToBackCount = marketRunner.AvailableToBackCount,
                    AvailableToLayCount = marketRunner.AvailableToLayCount,
                    TradedCount = marketRunner.TradedCount,
                };
                var result = new MarketRunnerSnap<MarketRunnerSnapAtTradedTVLTP>
                {
                    SelectionId = selectionId,
                    RunnerData = snap,
                };
                return Unsafe.As<MarketRunnerSnap<MarketRunnerSnapAtTradedTVLTP>, MarketRunnerSnap<TSnap>>(ref result);
            }

            throw new InvalidOperationException();
        }
    }
}

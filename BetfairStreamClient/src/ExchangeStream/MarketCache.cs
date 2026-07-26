using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BetfairStreamClient.ExchangeStream
{
    public class MarketCacheT<T> where T : struct, IClearable, IDisposable
    {
        public string MarketId { get; set; }
        private Dictionary<long, MarketRunner<T>> _runners = new Dictionary<long, MarketRunner<T>>();
        public Dictionary<long, MarketRunner<T>> Runners { get { return _runners; } }
        public int RunnerCount { get { return _runners.Count; } }
        public MarketCacheT(string marketId)
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
            if (typeof(T) == typeof(MarketRunnerBdat))
            {
                var target = (MarketRunner<MarketRunnerBdat>)(object)runner;
                ref MarketRunnerBdat marketRunner = ref Unsafe.AsRef(target.RunnerData);
                marketRunner.BestDisplayAvailableToBack = new LevelDelta[MarketRunnerBdat.MaxBdatCount];
                marketRunner.BestDisplayAvailableToLay = new LevelDelta[MarketRunnerBdat.MaxBdatCount];
                target.RunnerData = marketRunner;
            }
            else if (typeof(T) == typeof(MarketRunnerBat))
            {
                var target = (MarketRunner<MarketRunnerBat>)(object)runner;
                ref MarketRunnerBat marketRunner = ref Unsafe.AsRef(target.RunnerData);
                marketRunner.BestAvailableToBack = new LevelDelta[MarketRunnerBat.MaxBatCount];
                marketRunner.BestAvailableToLay = new LevelDelta[MarketRunnerBat.MaxBatCount];
                target.RunnerData = marketRunner;
            }
            else if (typeof(T) == typeof(MarketRunnerAt))
            {
                var target = (MarketRunner<MarketRunnerAt>)(object)runner;
                ref MarketRunnerAt marketRunner = ref Unsafe.AsRef(target.RunnerData);
                marketRunner.AvailableToBack = new PriceSizeDelta[MarketRunnerAt.MaxAtCount];
                marketRunner.AvailableToLay = new PriceSizeDelta[MarketRunnerAt.MaxAtCount];
                target.RunnerData = marketRunner;
            }
            else if (typeof(T) == typeof(MarketRunnerTraded))
            {
                var target = (MarketRunner<MarketRunnerTraded>)(object)runner;
                ref MarketRunnerTraded marketRunner = ref Unsafe.AsRef(target.RunnerData);
                marketRunner.Traded = new PriceSizeDelta[MarketRunnerTraded.MaxTradedCount];
                target.RunnerData = marketRunner;
            }
            else if (typeof(T) == typeof(MarketRunnerBdatTraded))
            {
                var target = (MarketRunner<MarketRunnerBdatTraded>)(object)runner;
                ref MarketRunnerBdatTraded marketRunner = ref Unsafe.AsRef(target.RunnerData);
                marketRunner.BestDisplayAvailableToBack = new LevelDelta[MarketRunnerBdat.MaxBdatCount];
                marketRunner.BestDisplayAvailableToLay = new LevelDelta[MarketRunnerBdat.MaxBdatCount];
                marketRunner.Traded = new PriceSizeDelta[MarketRunnerTraded.MaxTradedCount];
                target.RunnerData = marketRunner;
            }
            else if (typeof(T) == typeof(MarketRunnerBatTraded))
            {
                var target = (MarketRunner<MarketRunnerBatTraded>)(object)runner;
                MarketRunnerBatTraded marketRunner = target.RunnerData;
                marketRunner.BestAvailableToBack = new LevelDelta[MarketRunnerBat.MaxBatCount];
                marketRunner.BestAvailableToLay = new LevelDelta[MarketRunnerBat.MaxBatCount];
                marketRunner.Traded = new PriceSizeDelta[MarketRunnerTraded.MaxTradedCount];
                target.RunnerData = marketRunner;
            }
            else if (typeof(T) == typeof(MarketRunnerAtTraded))
            {
                var target = (MarketRunner<MarketRunnerAtTraded>)(object)runner;
                MarketRunnerAtTraded marketRunner = target.RunnerData;
                marketRunner.AvailableToBack = new PriceSizeDelta[MarketRunnerAt.MaxAtCount];
                marketRunner.AvailableToLay = new PriceSizeDelta[MarketRunnerAt.MaxAtCount];
                marketRunner.Traded = new PriceSizeDelta[MarketRunnerTraded.MaxTradedCount];
                target.RunnerData = marketRunner;
            }
            else if (typeof(T) == typeof(MarketRunnerBatTradedTVLTP))
            {
                var target = (MarketRunner<MarketRunnerBatTradedTVLTP>)(object)runner;
                MarketRunnerBatTradedTVLTP marketRunner = target.RunnerData;
                marketRunner.BestAvailableToBack = new LevelDelta[MarketRunnerBat.MaxBatCount];
                marketRunner.BestAvailableToLay = new LevelDelta[MarketRunnerBat.MaxBatCount];
                marketRunner.Traded = new PriceSizeDelta[MarketRunnerTraded.MaxTradedCount];
                target.RunnerData = marketRunner;
            }
            else if (typeof(T) == typeof(MarketRunnerBatTVLTP))
            {
                var target = (MarketRunner<MarketRunnerBatTVLTP>)(object)runner;
                MarketRunnerBatTVLTP marketRunner = target.RunnerData;
                marketRunner.BestAvailableToBack = new LevelDelta[MarketRunnerBat.MaxBatCount];
                marketRunner.BestAvailableToLay = new LevelDelta[MarketRunnerBat.MaxBatCount];
                target.RunnerData = marketRunner;
            }

            _runners[selectionId] = runner;
            return runner;
        }

        private LevelDelta[] RentAndCopy(LevelDelta[] levelDeltas, out int count)
        {
            ReadOnlySpan<LevelDelta> copy = levelDeltas.Length > 0 ? levelDeltas.AsSpan(0, levelDeltas.Length) : ReadOnlySpan<LevelDelta>.Empty;
            count = levelDeltas.Length;
            if (count == 0) return Array.Empty<LevelDelta>();
            LevelDelta[] buffer = ArrayPool<LevelDelta>.Shared.Rent(count);
            copy.CopyTo(buffer);
            return buffer;
        }

        private PriceSizeDelta[] RentAndCopy(PriceSizeDelta[] priceDeltas, out int count)
        {
            ReadOnlySpan<PriceSizeDelta> copy = priceDeltas.Length > 0 ? priceDeltas.AsSpan(0, priceDeltas.Length + 1) : ReadOnlySpan<PriceSizeDelta>.Empty;
            count = priceDeltas.Length;
            if (count == 0) return Array.Empty<PriceSizeDelta>();
            PriceSizeDelta[] buffer = ArrayPool<PriceSizeDelta>.Shared.Rent(count);
            copy.CopyTo(buffer);
            return buffer;
        }
        public MarketRunnerSnap<T> ExtractPooledSnapshot(long selectionId)
        {
            MarketRunner<T> runner = _runners[selectionId];

            if (typeof(T) == typeof(MarketRunnerSnapBdat))
            {
                var target = (MarketRunner<MarketRunnerBdat>)(object)runner;
                ref MarketRunnerBdat marketRunner = ref target.RunnerData;
                int bdatbCount = 0;
                LevelDelta[] bdatb = RentAndCopy(marketRunner.BestDisplayAvailableToBack, out bdatbCount);
                int bdatlCount = 0;
                LevelDelta[] bdatl = RentAndCopy(marketRunner.BestDisplayAvailableToLay, out bdatlCount);

                MarketRunnerSnapBdat snap = new MarketRunnerSnapBdat
                {
                    BestDisplayAvailableToBack = bdatb,
                    BestDisplayAvailableToLay = bdatl,
                    BestDisplayAvailableToBackCount = bdatbCount,
                    BestDisplayAvailableToLayCount = bdatlCount,
                };
                var result = new MarketRunnerSnap<MarketRunnerSnapBdat>
                {
                    SelectionId = selectionId,
                    RunnerData = snap,
                };
                return Unsafe.As<MarketRunnerSnap<MarketRunnerSnapBdat>, MarketRunnerSnap<T>>(ref result);
            }
            else if (typeof(T) == typeof(MarketRunnerBat))
            {
                var target = (MarketRunner<MarketRunnerBat>)(object)runner;
                ref MarketRunnerBat marketRunner = ref target.RunnerData;
                int bdatbCount = 0;
                LevelDelta[] bdatb = RentAndCopy(marketRunner.BestAvailableToBack, out bdatbCount);
                int bdatlCount = 0;
                LevelDelta[] bdatl = RentAndCopy(marketRunner.BestAvailableToLay, out bdatlCount);

                MarketRunnerSnapBat snap = new MarketRunnerSnapBat
                {
                    BestAvailableToBack = bdatb,
                    BestAvailableToLay = bdatl,
                    BestAvailableToBackCount = bdatbCount,
                    BestAvailableToLayCount = bdatlCount,
                };
                var result = new MarketRunnerSnap<MarketRunnerSnapBat>
                {
                    SelectionId = selectionId,
                    RunnerData = snap,
                };
                return Unsafe.As<MarketRunnerSnap<MarketRunnerSnapBat>, MarketRunnerSnap<T>>(ref result);
            }
            else if (typeof(T) == typeof(MarketRunnerBatTVLTP))
            {
                var target = (MarketRunner<MarketRunnerBatTVLTP>)(object)runner;
                ref MarketRunnerBatTVLTP marketRunner = ref target.RunnerData;
                int bdatbCount = 0;
                LevelDelta[] bdatb = RentAndCopy(marketRunner.BestAvailableToBack, out bdatbCount);
                int bdatlCount = 0;
                LevelDelta[] bdatl = RentAndCopy(marketRunner.BestAvailableToLay, out bdatlCount);

                MarketRunnerSnapBatTVLTP snap = new MarketRunnerSnapBatTVLTP
                {
                    BestAvailableToBack = bdatb,
                    BestAvailableToLay = bdatl,
                    BestAvailableToBackCount = bdatbCount,
                    BestAvailableToLayCount = bdatlCount,
                    TradedVolume = marketRunner.TradedVolume,
                    LastTradedPrice = marketRunner.LastTradedPrice,
                };
                var result = new MarketRunnerSnap<MarketRunnerSnapBatTVLTP>
                {
                    SelectionId = selectionId,
                    RunnerData = snap,
                };
                return Unsafe.As<MarketRunnerSnap<MarketRunnerSnapBatTVLTP>, MarketRunnerSnap<T>>(ref result);
            }
            else if (typeof(T) == typeof(MarketRunnerSnapAt))
            {
                var target = (MarketRunner<MarketRunnerAt>)(object)runner;
                ref MarketRunnerAt marketRunner = ref target.RunnerData;
                int atbCount = 0;
                PriceSizeDelta[] atb = RentAndCopy(marketRunner.AvailableToBack, out atbCount);
                int atlCount = 0;
                PriceSizeDelta[] atl = RentAndCopy(marketRunner.AvailableToLay, out atlCount);


                MarketRunnerSnapAt snap = new MarketRunnerSnapAt
                {
                    AvailableToBack = atb,
                    AvailableToLay = atl,

                };
                var result = new MarketRunnerSnap<MarketRunnerSnapAt>
                {
                    SelectionId = selectionId,
                    RunnerData = snap,
                };
                return Unsafe.As<MarketRunnerSnap<MarketRunnerSnapAt>, MarketRunnerSnap<T>>(ref result);
            }

            throw new InvalidOperationException();
        }
    }
}

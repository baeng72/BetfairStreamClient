using System.Buffers;

namespace BetfairStreamClient.Stream
{
    public class MarketRunnerCache
    {
        private const int MaxDepth = 20;
        private readonly PriceSize[][] _ladders = new PriceSize[6][];
        private readonly int[] _maxLevelsOccupied = new int[6];

        public MarketRunnerCache()
        {
            for (int i = 0; i < _ladders.Length; i++)
            {
                _ladders[i] = new PriceSize[MaxDepth];
                _maxLevelsOccupied[i] = -1;
            }
        }

        public void UpdateSingleSlot(int level, double price, double size, BetfairLadderType ladderType)
        {
            int typeIdx = (int)ladderType;
            if (level >= MaxDepth) return;

            

            if (size == 0)
            {
                _ladders[typeIdx][level] = default;
            }
            else
            {
                _ladders[typeIdx][level] = new PriceSize(price, size);
            }

            if (size > 0 && level > _maxLevelsOccupied[typeIdx])
            {
                _maxLevelsOccupied[typeIdx] = level;
            }
        }

        public ReadOnlySpan<PriceSize> GetLadderPrices(BetfairLadderType type)
        {
            int typeIdx = (int)type;
            int maxOccupied = _maxLevelsOccupied[typeIdx];
            return maxOccupied == -1 ? ReadOnlySpan<PriceSize>.Empty : _ladders[typeIdx].AsSpan(0, maxOccupied + 1);
        }

        public RunnerSnap ExtractPooledSnapshot(long selectionId)
        {
            return new RunnerSnap
            {
                SelectionId = selectionId,
                BestDisplayAvailableToBack = RentAndCopy(BetfairLadderType.Bdatb, out int bdatbCount),
                BdatbCount = bdatbCount,
                BestDisplayAvailableToLay = RentAndCopy(BetfairLadderType.Bdatl, out int bdatlCount),
                BdatlCount = bdatlCount,
                BestAvailableToBack = RentAndCopy(BetfairLadderType.Batb, out int batbCount),
                BatbCount = batbCount,
                BestAvailableToLay = RentAndCopy(BetfairLadderType.Batl, out int batlCount),
                BatlCount = batlCount
            };
        }

        private PriceSize[] RentAndCopy(BetfairLadderType type, out int count)
        {
            ReadOnlySpan<PriceSize> source = GetLadderPrices(type);
            count = source.Length;

            if (count == 0) return Array.Empty<PriceSize>();

            PriceSize[] buffer = ArrayPool<PriceSize>.Shared.Rent(count);
            source.CopyTo(buffer);
            return buffer;
        }
    }
//     public class MarketRunnerCache
// {
//     // Betfair depth rarely exceeds 10 levels for BDATB/BDATL.
//     // We allocation a fixed size of 20 to be safely padded.
//     private const int MaxDepth = 20;

//     // Fixed-size arrays allocated ONCE at startup. No future allocations.
//     private readonly PriceSize[] _backCache = new PriceSize[MaxDepth];
//     private readonly PriceSize[] _layCache = new PriceSize[MaxDepth];

//     // Track the actual active depth to avoid looping through empty array slots
//     public int MaxBackLevelOccupied { get; private set; } = -1;
//     public int MaxLayLevelOccupied { get; private set; } = -1;

//     public void UpdatePrices(List<List<double?>> deltas, bool isBack)
//     {
//         if (deltas == null) return;

//         PriceSize[] cache = isBack ? _backCache : _layCache;

//         // Use standard loops to avoid LINQ enumerator allocations
//         for (int i = 0; i < deltas.Count; i++)
//         {
//             var delta = deltas[i];
//             if (delta == null || delta.Count < 3) continue;

//             // Extract values using indexers safely
//             int level = (int)(delta[0] ?? 0);

//             // Safety check to prevent IndexOutOfRangeException
//             if (level >= MaxDepth) continue;

//             double price = delta[1] ?? 0.0;
//             double size = delta[2] ?? 0.0;

//             if (size == 0)
//             {
//                 // Clear the slot by overwriting with default (0, 0)
//                 cache[level] = default;
//             }
//             else
//             {
//                 // Overwrite the struct inline. Zero allocations.
//                 cache[level] = new PriceSize(price, size);
//             }

//             // Track active depth limits for fast reading
//             if (isBack && level > MaxBackLevelOccupied) MaxBackLevelOccupied = level;
//             if (!isBack && level > MaxLayLevelOccupied) MaxLayLevelOccupied = level;
//         }
//     }

//     // High-performance read access via ReadOnlySpan to prevent copying arrays
//     public ReadOnlySpan<PriceSize> GetBackPrices() => _backCache.AsSpan(0, MaxBackLevelOccupied + 1);
//     public ReadOnlySpan<PriceSize> GetLayPrices() => _layCache.AsSpan(0, MaxLayLevelOccupied + 1);
// }
}
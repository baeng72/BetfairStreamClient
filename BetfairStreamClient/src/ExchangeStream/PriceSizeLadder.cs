using BetfairStreamClient.Betting;
using BetfairStreamClient.ExchangeStream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BetfairStreamClient.ExchangeStream
{
    [InlineArray(350)]
    public struct InlineLookupBuffer
    {
        private int _element;
    }

    [InlineArray(350)]
    public struct InlineDataBuffer
    {
        private double _element;
    }

    [InlineArray(350)]
    public struct InlineIndexBuffer
    {
        private short _element;
    }

    public struct PriceSizeLadder
    {
        private InlineLookupBuffer _indexLookup;
        private InlineDataBuffer _denseSizes;
        // Tracks the Betfair tick index (0-349) for each active size element
        private InlineIndexBuffer _denseTickIndices;
        private int _activeCount;

        public void Initialize()
        {
            Span<int> lookupSpan = _indexLookup;
            lookupSpan.Fill(-1);
            _activeCount = 0;
        }

        public void Update(double price, double size)
        {
            Span<int> lookup = _indexLookup;
            Span<double> denseSizes = _denseSizes;
            Span<short> denseTickIndices = _denseTickIndices;

            int slot = GetIndex(price);
            if (slot == -1) return;//out of bounds
            int denseIdx = lookup[slot];
            if (size == 0.0)
            {
                if (denseIdx != -1)
                {
                    RemoveItemAt(slot, denseIdx, lookup, denseSizes, denseTickIndices);
                }
            }
            else if (denseIdx == -1)
            {
                lookup[slot] = _activeCount;
                denseSizes[_activeCount] = size;
                denseTickIndices[_activeCount] = (short)slot; // Store tick index
                _activeCount++;
            }
            else
            {
                denseSizes[denseIdx] = size;
            }
        }
        private void RemoveItemAt(int lookupSlot, int denseIdx, Span<int> lookup, Span<double> denseSizes, Span<short> denseTickIndices)
        {
            _activeCount--;
            lookup[lookupSlot] = -1;

            if (denseIdx < _activeCount)
            {
                double lastSize = denseSizes[_activeCount];
                short lastTickIdx = denseTickIndices[_activeCount];
                denseSizes[denseIdx] = lastSize;
                denseTickIndices[denseIdx] = lastTickIdx;

                for (int i = 0; i < lookup.Length; i++)
                {
                    if (lookup[i] == _activeCount)
                    {
                        lookup[i] = denseIdx;
                        break;
                    }
                }
            }
        }
        public readonly void CopyToPriceSizeSpan(Span<PriceSize> destination, bool descending)
        {
            if (destination.Length < _activeCount)
            {
                throw new ArgumentException("Destination span is too small to hold active items.");
            }

            // Rent tiny arrays on the stack to sort the active indices
            Span<short> sortedTicks = stackalloc short[_activeCount];
            Span<double> sortedSizes = stackalloc double[_activeCount];

            // Copy current state to the scratchpads
            ReadOnlySpan<short> currentTicks = _denseTickIndices;
            ReadOnlySpan<double> currentSizes = _denseSizes;
            currentTicks.Slice(0, _activeCount).CopyTo(sortedTicks);
            currentSizes.Slice(0, _activeCount).CopyTo(sortedSizes);

            // Dual-sort: Sorts the sizes array using the keys of the ticks array
            sortedTicks.Sort(sortedSizes);

            if (descending)
            {
                // For descending, read the sorted primitives backwards
                int destIdx = 0;
                for (int i = _activeCount - 1; i >= 0; i--)
                {
                    double exactPrice = GetPriceFromIndex(sortedTicks[i]);
                    destination[destIdx++] = new PriceSize(exactPrice, sortedSizes[i]);
                }
            }
            else
            {
                // For ascending, read them forwards
                for (int i = 0; i < _activeCount; i++)
                {
                    double exactPrice = GetPriceFromIndex(sortedTicks[i]);
                    destination[i] = new PriceSize(exactPrice, sortedSizes[i]);
                }
            }
        }

        //usage:
        //// Allocate space on the stack where it is perfectly safe
        //Span<double> activeSizesBuffer = stackalloc double;

        // Populate it directly out of your engine with zero allocations
        //variable.CopyActiveSizesTo(activeSizesBuffer);
        public readonly void GetActiveSizes(Span<double> destination)
        {
            ReadOnlySpan<double> denseSpan = _denseSizes;
            // Slice it to only contain active items, then copy directly to the destination
            denseSpan.Slice(0, _activeCount).CopyTo(destination);
        }

        public int LadderCount { get { return _activeCount; } }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetIndex(double price)
        {
            // Clean JSON float drift (e.g. 1.1100000000000001 -> 1.11)
            price = Math.Round(price, 2);

            if (price < 2.0) return (int)Math.Round((price - 1.0) * 100.0) - 1; // 1.01 to 1.99
            if (price < 3.0) return (int)Math.Round((price - 2.0) * 50.0) + 99;
            if (price < 4.0) return (int)Math.Round((price - 3.0) * 20.0) + 149;
            if (price < 6.0) return (int)Math.Round((price - 4.0) * 10.0) + 169;
            if (price < 10.0) return (int)Math.Round((price - 6.0) * 5.0) + 189;
            if (price < 20.0) return (int)Math.Round((price - 10.0) * 2.0) + 209;
            if (price < 30.0) return (int)Math.Round(price - 20.0) + 229;
            if (price < 50.0) return (int)Math.Round((price - 30.0) / 2.0) + 239;
            if (price < 100.0) return (int)Math.Round((price - 50.0) / 5.0) + 249;

            return (int)Math.Round((price - 100.0) / 10.0) + 259; // 100.0 to 1000.0
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double GetPriceFromIndex(int index)
        {
            // 1.01 to 1.99 (Indices 0 to 98) - Step 0.01
            if (index < 99)
                return Math.Round(1.0 + (index + 1) * 0.01, 2);

            // 2.0 to 2.98 (Indices 99 to 148) - Step 0.02
            if (index < 149)
                return Math.Round(2.0 + (index - 99) * 0.02, 2);

            // 3.0 to 3.95 (Indices 149 to 168) - Step 0.05
            if (index < 169)
                // Multiplying by 0.05 is mathematically equal to dividing by 20.0
                return Math.Round(3.0 + (index - 149) * 0.05, 2);

            // 4.0 to 5.9 (Indices 169 to 188) - Step 0.1
            if (index < 189)
                return Math.Round(4.0 + (index - 169) * 0.1, 1);

            // 6.0 to 9.8 (Indices 189 to 208) - Step 0.2
            if (index < 209)
                return Math.Round(6.0 + (index - 189) * 0.2, 1);

            // 10.0 to 19.5 (Indices 209 to 228) - Step 0.5
            if (index < 229)
                return Math.Round(10.0 + (index - 209) * 0.5, 1);

            // 20.0 to 29.0 (Indices 229 to 238) - Step 1.0
            if (index < 239)
                return Math.Round(20.0 + (index - 229) * 1.0, 1);

            // 30.0 to 48.0 (Indices 239 to 248) - Step 2.0
            if (index < 249)
                return Math.Round(30.0 + (index - 239) * 2.0, 1);

            // 50.0 to 95.0 (Indices 249 to 258) - Step 5.0
            if (index < 259)
                return Math.Round(50.0 + (index - 249) * 5.0, 1);

            // 100.0 to 1000.0 (Indices 259 to 349) - Step 10.0
            return Math.Round(100.0 + (index - 259) * 10.0, 1);
        }

        public void Clear()
        {
            Span<int> lookupSpan = _indexLookup;
            lookupSpan.Fill(-1);
            _activeCount = 0;
        }
    }

}

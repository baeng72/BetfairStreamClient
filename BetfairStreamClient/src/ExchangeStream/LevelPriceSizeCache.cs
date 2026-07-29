using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BetfairStreamClient.ExchangeStream
{
    [InlineArray(10)]
    public struct LevelPriceSizeBuffer
    {
        private LevelPriceSize _element0;
    }
    public struct LevelPriceSizeCache
    {
        private LevelPriceSizeBuffer _buffer;

        // Tracks current depth count up to 10
        public int Count { get; private set; }

        // Directly indexing into the inline array via Span conversion
        public readonly ref struct SpanView
        {
            private readonly ReadOnlySpan<LevelPriceSize> _span;
            public SpanView(ReadOnlySpan<LevelPriceSize> span) => _span = span;
            public readonly int Length => _span.Length;
            public readonly ref readonly LevelPriceSize this[int index] => ref _span[index];

            public readonly ReadOnlySpan<LevelPriceSize> Span => _span;
        }

        // Zero-allocation access to active items
        public readonly SpanView ActiveLevels
        {
            get
            {
                ReadOnlySpan<LevelPriceSize> fullSpan = MemoryMarshal.CreateReadOnlySpan(
                    ref Unsafe.As<LevelPriceSizeBuffer, LevelPriceSize>(ref Unsafe.AsRef(in _buffer)),
                    10
                );
                return new SpanView(fullSpan[..Count]);
            }
        }

        /// <summary>
        /// Updates a specific level index (0-9) inside the inline array cache.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update(int level, double price, double size)
        {
            // Guard against out-of-bounds stream anomalies
            if ((uint)level >= 10) return;

            // Get direct, zero-allocation reference to the inline array memory
            Span<LevelPriceSize> ladder = MemoryMarshal.CreateSpan(
                ref Unsafe.As<LevelPriceSizeBuffer, LevelPriceSize>(ref _buffer),
                10
            );

            if (size == 0.0)
            {
                // Betfair Stream API convention: Size of 0 means remove/clear this specific level
                ladder[level] = default;

                // If we cleared the highest active level, recalculate the tracked Count
                if (level >= Count - 1)
                {
                    RecalculateCount(ladder);
                }
            }
            else
            {
                // Assign the value directly into the embedded inline array memory slot
                ladder[level] = new LevelPriceSize(level, price, size);

                // Track how deep our valid elements go
                if (level >= Count)
                {
                    Count = level + 1;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RecalculateCount(ReadOnlySpan<LevelPriceSize> ladder)
        {
            // Scan backwards to find the true depth of the ladder
            for (int i = Count - 1; i >= 0; i--)
            {
                // A non-zero price or size indicates an active level slot
                if (ladder[i].Price != 0.0 || ladder[i].Size != 0.0)
                {
                    Count = i + 1;
                    return;
                }
            }
            Count = 0;
        }

        public void Clear()
        {
            Count = 0;
        }
    }
}

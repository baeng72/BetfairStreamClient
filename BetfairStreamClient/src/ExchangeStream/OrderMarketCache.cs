using BetfairStreamClient.ExchangeStream;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetfairStreamClient.ExchangeStream
{
    public class OrderMarketCache : IClearable, IDisposable //we could pass a specific order type, less data overhead (i.e, don't need bsp) but for now...
    {
        public string MarketId { get; set; }
        private Dictionary<long, OrderRunnerCache> _runners = new Dictionary<long, OrderRunnerCache>();

        public Dictionary<long, OrderRunnerCache> Runners { get { return _runners; } }
        public int RunnerCount { get { return _runners.Count; } }

        public OrderMarketCache(string marketId)
        {
            MarketId = marketId;
        }

        public OrderRunnerCache GetOrCreateRunnerCache(string marketId, long selectionId)
        {
            if (_runners.TryGetValue(selectionId, out var runner)) return runner;
            runner = new OrderRunnerCache(selectionId);
            _runners[selectionId] = runner;
            return runner;
        }

        private Order[] RentAndCopy(Order[] orders, out int count)
        {
            ReadOnlySpan<Order> copy = orders.Length > 0 ? orders.AsSpan(0, orders.Length) : ReadOnlySpan<Order>.Empty;
            count = orders.Length;
            if (count == 0) return Array.Empty<Order>();
            Order[] buffer = ArrayPool<Order>.Shared.Rent(count);
            copy.CopyTo(buffer);
            return buffer;
        }
        private PriceSizeDelta[] RentAndCopy(PriceSizeDelta[] orders, out int count)
        {
            ReadOnlySpan<PriceSizeDelta> copy = orders.Length > 0 ? orders.AsSpan(0, orders.Length) : ReadOnlySpan<PriceSizeDelta>.Empty;
            count = orders.Length;
            if (count == 0) return Array.Empty<PriceSizeDelta>();
            PriceSizeDelta[] buffer = ArrayPool<PriceSizeDelta>.Shared.Rent(count);
            copy.CopyTo(buffer);
            return buffer;
        }
        public OrderRunnerSnap ExtractPooledSnapshot(long selectionId)
        {
            OrderRunnerCache runner = _runners[selectionId];
            int orderCount = 0;
            var orders = RentAndCopy(runner.UnmatchedOrders, out orderCount);
            int matchedBacksCount = 0;
            var matchedBacks = RentAndCopy(runner.MatchedBacks, out matchedBacksCount);
            int matchedLaysCount = 0;
            var matchedLays = RentAndCopy(runner.MatchedLays, out matchedLaysCount);
            return new OrderRunnerSnap{
                SelectionId=selectionId,
                UnmatchedOrders = orders,
                UnmatchedOrderCount=orderCount,
                MatchedBacks=matchedBacks,
                MatchedBacksCount=matchedBacksCount,
                MatchedLays=matchedLays,
                MatchedLaysCount=matchedLaysCount
            };
        }
        public void Clear()
        {
            _runners.Clear();
        }
        public void Dispose() { }
    }
}

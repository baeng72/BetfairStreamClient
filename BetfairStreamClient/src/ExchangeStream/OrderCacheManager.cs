using System.Buffers;
using System.Collections.Concurrent;

namespace BetfairStreamClient.ExchangeStream
{
    public class OrderCacheManager
    {
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<long, OrderRunnerCache>> _orderCache = new();
        
        public event EventHandler<OrderMarketSnap>? OrderSnapshotUpdated;

        public OrderRunnerCache GetOrCreateRunnerCache(string marketId, long selectionId)
        {
            var runners = _orderCache.GetOrAdd(marketId, _ => new ConcurrentDictionary<long, OrderRunnerCache>());
            return runners.GetOrAdd(selectionId, _ => new OrderRunnerCache());
        }

        public void ClearCacheForMarket(string marketId)
        {
            if (_orderCache.TryRemove(marketId, out var runners))
            {
                runners.Clear();
            }
        }

        public void ProcessAndBroadcast(string marketId)
        {
            if (!_orderCache.TryGetValue(marketId, out var runners)) return;

            // Determine maximum potential slots needed across all runner books
            int maxPotentialOrders = 0;
            int totalBackCount = 0;
            int totalLayCount = 0;
            foreach (var kvp in runners) {
                maxPotentialOrders += kvp.Value.ActiveCount;
                
            }
            if (maxPotentialOrders == 0) return;

            OrderSnap[] pooledOrders = ArrayPool<OrderSnap>.Shared.Rent(maxPotentialOrders);
            int writeIndex = 0;
            foreach (var kvp in runners)
            {
                writeIndex = kvp.Value.CopyActiveOrdersTo(pooledOrders, writeIndex);                
            }

            var snap = new OrderMarketSnap
            {
                MarketId = marketId,
                Orders = pooledOrders,
                OrderCount = writeIndex
            };

            OrderSnapshotUpdated?.Invoke(this, snap);
        }

        public OrderMarketSnap? GetMarketSnap(string marketId)
        {
            if (!_orderCache.TryGetValue(marketId, out var runners)) return null;

            // Determine maximum potential slots needed across all runner books
            int maxPotentialOrders = 0;
            foreach (var kvp in runners) maxPotentialOrders += kvp.Value.ActiveCount;
            if (maxPotentialOrders == 0) return null;

            OrderSnap[] pooledOrders = ArrayPool<OrderSnap>.Shared.Rent(maxPotentialOrders);
            int writeIndex = 0;

            foreach (var kvp in runners)
            {
                writeIndex = kvp.Value.CopyActiveOrdersTo(pooledOrders, writeIndex);
            }

            var snap = new OrderMarketSnap
            {
                MarketId = marketId,
                Orders = pooledOrders,
                OrderCount = writeIndex
            };
            return snap;
        }
    }
}
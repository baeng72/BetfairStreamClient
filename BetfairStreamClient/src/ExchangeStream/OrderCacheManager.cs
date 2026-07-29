using System.Buffers;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;

namespace BetfairStreamClient.ExchangeStream
{
    public class OrderCacheManager : IDisposable, IClearable
    {
        private readonly ConcurrentDictionary<string, OrderMarketCache> _markets = new ConcurrentDictionary<string, OrderMarketCache>();
        public event EventHandler<OrderMarketChangeNotification>? OrderNotificationReceived;
        public OrderMarketCache GetOrCreateMarket(string marketId)
        {
            var marketCache = _markets.GetOrAdd(marketId, _ => new OrderMarketCache(marketId));
            return marketCache;
        }
        public void Clear()
        {
            _markets.Clear();
        }
        public void Dispose() { }
        public void ProcessAndBroadcast(string marketId, DateTime timeStamp)
        {
            if (!_markets.TryGetValue(marketId, out var marketCache)) return;
            int totalRunners = marketCache.RunnerCount;
            if (totalRunners == 0) return;
            int index = 0;
            OrderRunnerSnap[] pooledRunners = ArrayPool<OrderRunnerSnap>.Shared.Rent(totalRunners);

            foreach (var kvp in marketCache.Runners)
            {
                // Cast directly to the generic struct (no double casting needed if setup correctly)
                pooledRunners[index++] = (OrderRunnerSnap)marketCache.ExtractPooledSnapshot(kvp.Key);
            }            

            var snap = new OrderMarketSnap
            {
                
                Runners = pooledRunners,
                RunnerCount = totalRunners
            };

            var notification = new OrderMarketChangeNotification
            {
                MarketId = marketId,
                TimeStamp = timeStamp,
                OrderSnap = new OrderMarketSnap { RunnerCount = totalRunners, Runners = pooledRunners }
            };

            OrderNotificationReceived?.Invoke(this, notification);
            notification.Dispose();
        }

        public OrderMarketSnap GetOrderMarketSnap(string marketId)
        {
            _markets.TryGetValue(marketId, out var marketCache);
            int totalRunners = marketCache.RunnerCount;            
            int index = 0;
            OrderRunnerSnap[] pooledRunners = ArrayPool<OrderRunnerSnap>.Shared.Rent(totalRunners);

            foreach (var kvp in marketCache.Runners)
            {
                // Cast directly to the generic struct (no double casting needed if setup correctly)
                pooledRunners[index++] = (OrderRunnerSnap)marketCache.ExtractPooledSnapshot(kvp.Key);
            }

            var snap = new OrderMarketSnap
            {

                Runners = pooledRunners,
                RunnerCount = totalRunners
            };
            return snap;            
        }

        //    public OrderMarketSnap? GetMarketSnap(string marketId)
        //    {
        //        if (!_orderCache.TryGetValue(marketId, out var runners)) return null;

        //        // Determine maximum potential slots needed across all runner books
        //        int maxPotentialOrders = 0;
        //        foreach (var kvp in runners) maxPotentialOrders += kvp.Value.ActiveCount;
        //        if (maxPotentialOrders == 0) return null;

        //        OrderSnap[] pooledOrders = ArrayPool<OrderSnap>.Shared.Rent(maxPotentialOrders);
        //        int writeIndex = 0;

        //        foreach (var kvp in runners)
        //        {
        //            writeIndex = kvp.Value.CopyActiveOrdersTo(pooledOrders, writeIndex);
        //        }

        //        var snap = new OrderMarketSnap
        //        {
        //            MarketId = marketId,
        //            Orders = pooledOrders,
        //            OrderCount = writeIndex
        //        };
        //        return snap;
        //    }
    }
}
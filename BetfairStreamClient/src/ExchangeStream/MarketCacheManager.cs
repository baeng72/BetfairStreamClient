using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
namespace BetfairStreamClient.ExchangeStream
{


    public class MarketCacheManager<T> where T : struct, IDisposable, IClearable
    {
        private readonly ConcurrentDictionary<string, MarketCacheT<T>> _markets = new ConcurrentDictionary<string, MarketCacheT<T>>();
        public event EventHandler<MarketChangeNotification<T>>? MarketNotificationReceived;
        public MarketCacheT<T> GetOrCreateMarket(string marketId)
        {
            var marketCache = _markets.GetOrAdd(marketId, _ => new MarketCacheT<T>(marketId));
            return marketCache;
        }
        public void Clear()
        {
            _markets.Clear();
        }
        public void ProcessAndBroadcast(string marketId, DateTime timeStamp, MarketDefinition? definition)
        {
            if (!_markets.TryGetValue(marketId, out var marketCache)) return;

            int totalRunners = marketCache.RunnerCount;
            if (totalRunners == 0) return;
            int index = 0;
            MarketRunnerSnap<T>[] pooledRunners = ArrayPool<MarketRunnerSnap<T>>.Shared.Rent(totalRunners);

            foreach (var kvp in marketCache.Runners)
            {
                // Cast directly to the generic struct (no double casting needed if setup correctly)
                pooledRunners[index++] = (MarketRunnerSnap<T>)marketCache.ExtractPooledSnapshot(kvp.Key);
            }

            // Create the notification directly with T
            var notification = new MarketChangeNotification<T>
            {
                MarketId = marketId,
                MarketDefinition = definition,
                Timestamp = timeStamp,
                MarketSnap = new MarketSnap<T> { RunnerPrices = pooledRunners, RunnerCount = totalRunners},                
            };

            // Clean, allocation-free execution without boxing/casting tricks
            MarketNotificationReceived?.Invoke(this, notification);
            notification.Dispose();
        }

        

        public void Dispose() { }

    }

}


using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
namespace BetfairStreamClient.ExchangeStream
{


    public class MarketCacheManager<T, TSnap> where T : struct, IDisposable, IClearable where TSnap : struct, IDisposable, IClearable
    {
        private readonly ConcurrentDictionary<string, MarketCache<T, TSnap>> _markets = new ConcurrentDictionary<string, MarketCache<T, TSnap>>();
        public event EventHandler<MarketChangeNotification<TSnap>>? MarketNotificationReceived;
        public MarketCache<T, TSnap> GetOrCreateMarket(string marketId)
        {
            var marketCache = _markets.GetOrAdd(marketId, _ => new MarketCache<T, TSnap>(marketId));
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
            MarketRunnerSnap<TSnap>[] pooledRunners = ArrayPool<MarketRunnerSnap<TSnap>>.Shared.Rent(totalRunners);

            foreach (var kvp in marketCache.Runners)
            {
                // Cast directly to the generic struct (no double casting needed if setup correctly)
                pooledRunners[index++] = (MarketRunnerSnap<TSnap>)marketCache.ExtractPooledSnapshot(kvp.Key);
            }

            // Create the notification directly with T
            var notification = new MarketChangeNotification<TSnap>
            {
                MarketId = marketId,
                //MarketDefinition = definition,
                Timestamp = timeStamp,
                TradedVolume = marketCache.TradedVolume,
                MarketSnap = new MarketSnap<TSnap> { RunnerPrices = pooledRunners, RunnerCount = totalRunners, MarketDefinition = definition},                
            };

            // Clean, allocation-free execution without boxing/casting tricks
            MarketNotificationReceived?.Invoke(this, notification);
            notification.Dispose();
        }

        public MarketSnap<TSnap> GetMarketSnap(string marketId)
        {
            _markets.TryGetValue(marketId, out var marketCache);
            int totalRunners = marketCache.RunnerCount;
            
            int index = 0;
            MarketRunnerSnap<TSnap>[] pooledRunners = ArrayPool<MarketRunnerSnap<TSnap>>.Shared.Rent(totalRunners);

            foreach (var kvp in marketCache.Runners)
            {
                // Cast directly to the generic struct (no double casting needed if setup correctly)
                pooledRunners[index++] = (MarketRunnerSnap<TSnap>)marketCache.ExtractPooledSnapshot(kvp.Key);
            }
            return new MarketSnap<TSnap>
            {
                RunnerPrices = pooledRunners,
                RunnerCount = totalRunners
            };
        }

        public void Dispose() { }

    }

}


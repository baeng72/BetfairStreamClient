using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
namespace BetfairStreamClient.Stream
{


    public class MarketCacheManager
    {
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<long, MarketRunnerCache>> _marketCache = new();
        private readonly ConcurrentDictionary<string, MarketDefinition> _marketDefinitions = new();
        public event EventHandler<MarketChangeNotification>? MarketNotificationReceived;
        

        public MarketRunnerCache GetOrCreateRunnerCache(string marketId, long selectionId)
        {
            if (selectionId == 0)
            {
                int x = 0;
            }
            var runnersInMarket = _marketCache.GetOrAdd(marketId, _ => new ConcurrentDictionary<long, MarketRunnerCache>());
            return runnersInMarket.GetOrAdd(selectionId, _ => new MarketRunnerCache());
        }

        public void ClearCacheForMarket(string marketId)
        {
            if (_marketCache.TryRemove(marketId, out var runnersInMarket))
            {
                runnersInMarket.Clear();
            }
        }

        
        public void ProcessAndBroadcast(string marketId, MarketDefinition? definition)
        {
            if (!_marketCache.TryGetValue(marketId, out var runnersInMarket)) return;
            if (definition != null)
            {
                _marketDefinitions[marketId] = definition;
            }
            int totalRunners = runnersInMarket.Count;
            if (totalRunners == 0) return;

            RunnerSnap[] pooledRunners = ArrayPool<RunnerSnap>.Shared.Rent(totalRunners);
            int index = 0;
            foreach (var kvp in runnersInMarket)
            {
                pooledRunners[index++] = kvp.Value.ExtractPooledSnapshot(kvp.Key);
            }

            var notification = new MarketChangeNotification
            {
                MarketId = marketId,
                MarketDefinition = definition,
                PriceSnapshot = new MarketSnap { MarketId = marketId, Timestamp = DateTime.UtcNow, Runners = pooledRunners, RunnerCount = totalRunners }
            };

            MarketNotificationReceived?.Invoke(this, notification);
        }

        public MarketSnap? GetMarketSnap(string marketId)
        {
            if (!_marketCache.TryGetValue(marketId, out var runnersInMarket)) return null;
            int totalRunners = runnersInMarket.Count;
            if (totalRunners == 0) return null;

            RunnerSnap[] pooledRunners = ArrayPool<RunnerSnap>.Shared.Rent(totalRunners);
            int index = 0;
            foreach (var kvp in runnersInMarket)
            {
                pooledRunners[index++] = kvp.Value.ExtractPooledSnapshot(kvp.Key);
            }

            return new MarketSnap { MarketId = marketId, Timestamp = DateTime.UtcNow, Runners = pooledRunners, RunnerCount = totalRunners };
        }

        public MarketDefinition? GetMarketDefinition(string marketId)
        {
            if(!_marketDefinitions.TryGetValue(marketId, out var definition)) return null;
            return definition;
        }
    }

    
}


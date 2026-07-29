using BetfairStreamClient.tests.ExchangeStream.Cache;
using BetfairStreamClient.tests.ExchangeStream.Model;
using BetfairStreamClient.tests.ExchangeStream.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetfairStreamClient.tests.ExchangeStream
{
    /// <summary>
    /// Simple ESA Cache implementation - only what's necessary for testing
    /// and caches the streams of data.
    /// </summary>
    public class ClientCache : Protocol.IChangeMessageHandler
    {
        private readonly MarketCache _marketCache = new MarketCache();
        private readonly OrderCache _orderCache = new OrderCache();

        public ClientCache() { }

        /// <summary>
        /// The cache of all subscribed markets
        /// </summary>
        public MarketCache MarketCache
        {
            get
            {
                return _marketCache;
            }
        }

        /// <summary>
        /// The cache of all subscribed orders
        /// </summary>
        public OrderCache OrderCache
        {
            get
            {
                return _orderCache;
            }
        }


        #region IChangeMessageHandler

        void IChangeMessageHandler.OnMarketChange(ChangeMessage<MarketChange> change)
        {
            _marketCache.OnMarketChange(change);
        }

        void IChangeMessageHandler.OnOrderChange(ChangeMessage<OrderMarketChange> change)
        {
            _orderCache.OnOrderChange(change);
        }

        void IChangeMessageHandler.OnErrorStatusNotification(StatusMessage message)
        {

        }
        #endregion
    }
}

using BetfairStreamClient.ExchangeStream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BetfairStreamClient.tests.ExchangeStream.Cache
{
    /// <summary>
    /// Cached state of the runner
    /// </summary>
    public class OrderMarketRunner
    {
        private readonly Cache.OrderMarket _market;
        private readonly RunnerId _runnerId;

        private OrderMarketRunnerSnap _snap;
        private PriceSizeLadder _matchedLay = PriceSizeLadder.NewLay();
        private PriceSizeLadder _matchedBack = PriceSizeLadder.NewBack();
        private Dictionary<string, Model.Order> _unmatchedOrders = new Dictionary<string, Model.Order>();


        public OrderMarketRunner(Cache.OrderMarket market, RunnerId runnerId)
        {
            _market = market;
            _runnerId = runnerId;
        }

        internal void OnOrderRunnerChange(Model.OrderRunnerChange orderRunnerChange)
        {
            bool isImage = orderRunnerChange.FullImage == true;

            if (isImage)
            {
                //image so clear down
                _unmatchedOrders.Clear();
            }

            if (orderRunnerChange.Uo != null)
            {
                //have order changes
                foreach (Model.Order order in orderRunnerChange.Uo)
                {
                    _unmatchedOrders[order.Id] = order;
                }
            }


            OrderMarketRunnerSnap newSnap = new OrderMarketRunnerSnap();
            newSnap.RunnerId = _runnerId;
            newSnap.UnmatchedOrders = new Dictionary<string, Model.Order>(_unmatchedOrders);

            newSnap.MatchedLay = _matchedLay.OnPriceChange(isImage, orderRunnerChange.Ml);
            newSnap.MatchedBack = _matchedBack.OnPriceChange(isImage, orderRunnerChange.Mb);

            _snap = newSnap;
        }

        public OrderMarket Market
        {
            get
            {
                return _market;
            }
        }

        public RunnerId RunnerId
        {
            get
            {
                return _runnerId;
            }
        }

        /// <summary>
        /// Takes or returns an existing immutable snap of the runner.
        /// </summary>
        public OrderMarketRunnerSnap Snap
        {
            get
            {
                return _snap;
            }
        }

        public override string ToString()
        {
            return _snap == null ? "null" : _snap.ToString();
        }
    }
}

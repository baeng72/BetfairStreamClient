
using System.Drawing;
using System.Runtime.Serialization;

namespace BetfairStreamClient.ExchangeStream
{
    public struct OrderRunnerCache : IDisposable, IClearable
    {
        private const int MaxOrderCount = 20;
        private const int MaxBetCount = 20;
        public long SelectionId;
        public Order[] UnmatchedOrders;
        public int UnmatchedOrdersCount;
        public PriceSizeDelta[] MatchedBacks;
        public int MatchedBacksCount;
        public PriceSizeDelta[] MatchedLays;
        public int MatchedLaysCount;
        public OrderRunnerCache(long selectionId)
        {
            SelectionId = selectionId;
            UnmatchedOrders = new Order[MaxOrderCount];
            MatchedBacks = new PriceSizeDelta[MaxOrderCount];
            MatchedLays = new PriceSizeDelta[MaxBetCount];
        }

        public void AddOrder(Order order)
        {
            UnmatchedOrders[UnmatchedOrdersCount++] = order;
        }

        public void AddMatchedBacks(double price, double size)
        {
            MatchedBacks[MatchedBacksCount++] = new PriceSizeDelta(price, size);
        }
        public void AddMatchedLays(double price, double size)
        {
            MatchedLays[MatchedLaysCount++] = new PriceSizeDelta((double)price, size);
        }
        public void Clear()
        {
            UnmatchedOrdersCount = 0;
            MatchedBacksCount = 0;
            MatchedLaysCount = 0;
        }

        public void Dispose()
        {
        }
    }
}
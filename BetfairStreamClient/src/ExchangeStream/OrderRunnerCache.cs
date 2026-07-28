
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
        public PriceSize[] MatchedBacks;
        public int MatchedBacksCount;
        public PriceSize[] MatchedLays;
        public int MatchedLaysCount;
        public OrderRunnerCache(long selectionId)
        {
            SelectionId = selectionId;
            UnmatchedOrders = new Order[MaxOrderCount];
            MatchedBacks = new PriceSize[MaxOrderCount];
            MatchedLays = new PriceSize[MaxBetCount];
        }

        public void AddOrder(Order order)
        {
            UnmatchedOrders[UnmatchedOrdersCount++] = order;
        }

        public void AddMatchedBacks(double price, double size)
        {
            MatchedBacks[MatchedBacksCount++] = new PriceSize(price, size);
        }
        public void AddMatchedLays(double price, double size)
        {
            MatchedLays[MatchedLaysCount++] = new PriceSize((double)price, size);
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
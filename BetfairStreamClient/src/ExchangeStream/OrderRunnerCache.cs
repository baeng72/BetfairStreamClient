
using System.Drawing;
using System.Runtime.Serialization;

namespace BetfairStreamClient.ExchangeStream
{
    public struct OrderRunnerCache : IDisposable, IClearable
    {
        private const int MaxOrderCount = 20;        
        public long SelectionId;
        public Order[] UnmatchedOrders;
        public int UnmatchedOrdersCount;
        public PriceSizeLadder MatchedBacks;
        public int MatchedBacksCount;
        public PriceSizeLadder MatchedLays;
        public int MatchedLaysCount;
        public OrderRunnerCache(long selectionId)
        {
            SelectionId = selectionId;
            UnmatchedOrders = new Order[MaxOrderCount];
            
        }

        public void AddOrder(Order order)
        {
            if(UnmatchedOrdersCount<MaxOrderCount)
                UnmatchedOrders[UnmatchedOrdersCount++] = order;
        }

        public void AddMatchedBacks(double price, double size)
        {
            MatchedBacks.Update(price, size);
            
        }
        public void AddMatchedLays(double price, double size)
        {
            MatchedLays.Update(price, size);
        }
        public void Clear()
        {
            UnmatchedOrdersCount = 0;
            MatchedBacksCount = 0;
            MatchedLaysCount = 0;
            MatchedBacks.Clear();
            MatchedLays.Clear();
            
        }

        public void Dispose()
        {
        }
    }
}
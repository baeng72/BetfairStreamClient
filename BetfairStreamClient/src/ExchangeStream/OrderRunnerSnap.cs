using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetfairStreamClient.ExchangeStream
{
    public struct OrderRunnerSnap : IDisposable, IClearable
    {
        public long SelectionId;
        public Order[] UnmatchedOrders;
        public int UnmatchedOrderCount;
        public PriceSize[] MatchedBacks;
        public int MatchedBacksCount;
        public PriceSize[] MatchedLays;
        public int MatchedLaysCount;

        public void Dispose()
        {
            if(UnmatchedOrders != null)
            {
                ArrayPool<Order>.Shared.Return(UnmatchedOrders);
            }
            if (MatchedBacks != null)
            {
                ArrayPool<PriceSize>.Shared.Return(MatchedBacks);
            }
            if (MatchedLays != null)
            {
                ArrayPool<PriceSize>.Shared.Return(MatchedLays);
            }
        }

        public void Clear()
        {
            UnmatchedOrderCount = 0;
            MatchedBacksCount = 0;
            MatchedLaysCount = 0;
        }
    }
}

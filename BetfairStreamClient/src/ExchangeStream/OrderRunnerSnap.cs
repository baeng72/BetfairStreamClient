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
        public PriceSizeDelta[] MatchedBacks;
        public int MatchedBacksCount;
        public PriceSizeDelta[] MatchedLays;
        public int MatchedLaysCount;

        public void Dispose()
        {
            if(UnmatchedOrders != null)
            {
                ArrayPool<Order>.Shared.Return(UnmatchedOrders);
            }
            if (MatchedBacks != null)
            {
                ArrayPool<PriceSizeDelta>.Shared.Return(MatchedBacks);
            }
            if (MatchedLays != null)
            {
                ArrayPool<PriceSizeDelta>.Shared.Return(MatchedLays);
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

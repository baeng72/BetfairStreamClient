using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetfairStreamClient.ExchangeStream
{
    public struct OrderMarketChangeNotification : IDisposable, IClearable
    {
        public string MarketId;
        public DateTime TimeStamp;
        public OrderMarketSnap OrderSnap;
        public void Dispose()
        {
            OrderSnap.Dispose();            
        }
        public void Clear()
        {

        }
    }
}

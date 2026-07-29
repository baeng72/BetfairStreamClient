using System;
using System.Buffers;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetfairStreamClient.ExchangeStream
{
    public struct MarketChangeNotification<T> : IDisposable, IClearable where T : struct, IDisposable, IClearable
    {
        public string MarketId { get; init; }
        //public MarketDefinition? MarketDefinition { get; init; }

        public MarketSnap<T> MarketSnap { get; init; }

        public DateTime Timestamp { get; init; }

        public void Dispose()
        {
            MarketSnap.Dispose();            
        }
        public void Clear() { }
    }
}

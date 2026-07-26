using BetfairStreamClient.ExchangeStream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetfairStreamClient.ExchangeStream
{
    public struct MarketSnap<T> where T : struct, IDisposable, IClearable
    {
        public MarketRunnerSnap<T>[] RunnerPrices { get; init; }

        public int RunnerCount { get; init; }

        public void Dispose()
        {
            for (int i = 0; i < RunnerCount; i++)
            {
                RunnerPrices[i].Dispose();
            }
        }
        public void Clear() { }
    }
}

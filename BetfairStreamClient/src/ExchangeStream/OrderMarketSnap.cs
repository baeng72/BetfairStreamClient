using BetfairStreamClient.ExchangeStream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetfairStreamClient.ExchangeStream
{
    public struct OrderMarketSnap : IDisposable, IClearable
    {        
        public OrderRunnerSnap[] Runners { get; init; }

        public int RunnerCount { get; init; }

        public void Dispose()
        {
            for (int i = 0; i < RunnerCount; i++)
            {
                Runners[i].Dispose();
            }
        }
        public void Clear() { }
    }
}

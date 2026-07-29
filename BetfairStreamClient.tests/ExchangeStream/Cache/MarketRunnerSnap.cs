using BetfairStreamClient.ExchangeStream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetfairStreamClient.tests.ExchangeStream.Cache
{
    public class MarketRunnerSnap
    {
        public Cache.RunnerId RunnerId { get; internal set; }
        public Model.RunnerDefinition Definition { get; internal set; }
        public Cache.MarketRunnerPrices Prices { get; internal set; }

        public override string ToString()
        {
            return "MarketRunnerSnap{" +
                    "runnerId=" + RunnerId +
                    ", prices=" + Prices +
                    ", definition=" + Definition +
                    '}';
        }
    }
}

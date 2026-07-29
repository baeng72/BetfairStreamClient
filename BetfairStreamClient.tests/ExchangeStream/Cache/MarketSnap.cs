using BetfairStreamClient.ExchangeStream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetfairStreamClient.tests.ExchangeStream.Cache
{
    public class MarketSnap
    {
        public Model.MarketDefinition MarketDefinition { get; internal set; }
        public string MarketId { get; internal set; }
        public IList<MarketRunnerSnap> MarketRunners { get; internal set; }
        public double TradedVolume { get; internal set; }

        public override string ToString()
        {
            return "MarketSnap{" +
                "MarketId=" + MarketId +
                ", MarketDefinition=" + MarketDefinition +
                ", MarketRunners=" + String.Join(", ", MarketRunners) +
                ", TradedVolume=" + TradedVolume +
                "}";
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BetfairStreamClient.Betting
{
    public sealed class ExchangePrices
    {
        [JsonPropertyName("availableToBack")]
        public List<PriceSize>? AvailableToBack { get; set; }

        [JsonPropertyName("availableToLay")]
        public List<PriceSize>? AvailableToLay { get; set; }

        [JsonPropertyName("tradedVolume")]
        public List<PriceSize>? TradedVolume { get; set; }
    }
 

}

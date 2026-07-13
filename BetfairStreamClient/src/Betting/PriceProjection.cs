using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BFBot.Betting
{
    public sealed class PriceProjection
    {
        [JsonPropertyName("priceData")]
        public List<PriceData> PriceData { get; set; }

        [JsonPropertyName("virtualise")]
        public bool? Virtualise { get; set; }

        [JsonPropertyName("rolloverStakes")]
        public bool? RolloverStakes { get; set; }
    }
}

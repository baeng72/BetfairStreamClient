using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BFBot.Betting
{
    public sealed class RunnerBook
    {
        [JsonPropertyName("selectionId")]
        public long SelectionId { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = "";

        [JsonPropertyName("lastPriceTraded")]
        public double? LastPriceTraded { get; set; }

        [JsonPropertyName("totalMatched")]
        public double? TotalMatched { get; set; }

        [JsonPropertyName("ex")]
        public ExchangePrices? ExchangePrices { get; set; }
    }

}

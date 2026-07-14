using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BetfairStreamClient.Betting
{
    public sealed class RunnerBook
    {
        [JsonPropertyName("selectionId")]
        public long SelectionId { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = null!;

        [JsonPropertyName("lastPriceTraded")]
        public double? LastPriceTraded { get; set; }

        [JsonPropertyName("totalMatched")]
        public double? TotalMatched { get; set; }

        [JsonPropertyName("ex")]
        public ExchangePrices? ExchangePrices { get; set; }
    }

}

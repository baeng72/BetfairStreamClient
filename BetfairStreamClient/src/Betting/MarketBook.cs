using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BetfairStreamClient.Betting
{
    public sealed class MarketBook
    {
        [JsonPropertyName("marketId")]
        public string MarketId { get; set; } = "";

        [JsonPropertyName("status")]
        public string Status { get; set; } = ""; // e.g. OPEN, SUSPENDED, CLOSED

        [JsonPropertyName("betDelay")]
        public int BetDelay { get; set; }

        [JsonPropertyName("inplay")]
        public bool InPlay { get; set; }

        [JsonPropertyName("numberOfRunners")]
        public int NumberOfRunners { get; set; }

        [JsonPropertyName("totalMatched")]
        public double? TotalMatched { get; set; }

        [JsonPropertyName("runners")]
        public List<RunnerBook>? Runners { get; set; }
    }

}

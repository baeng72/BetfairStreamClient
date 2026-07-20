
using System.Text.Json.Serialization;

namespace BetfairStreamClient.Betting
{
    public sealed class MarketCatalogue
    {
        [JsonPropertyName("marketId")]
        public string MarketId { get; set; } = "";

        [JsonPropertyName("marketName")]
        public string MarketName { get; set; } = "";

        [JsonPropertyName("marketStartTime")]
        public DateTime? MarketStartTime { get; set; }

        [JsonPropertyName("totalMatched")]
        public double? TotalMatched { get; set; }

        [JsonPropertyName("competition")]
        public Competition? Competition { get; set; }

        [JsonPropertyName("event")]
        public MarketEvent? Event { get; set; }

        [JsonPropertyName("eventType")]
        public EventType? EventType { get; set; }

        [JsonPropertyName("runners")]
        public List<RunnerCatalog>? Runners { get; set; }
    }

}

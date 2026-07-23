using System.Text.Json.Serialization;

namespace BetfairStreamClient.ExchangeStream
{
    public class OrderFilter
    {
        [JsonPropertyName("accountIds")]
        public List<long?> AccountIds { get; set; } = null!;

        [JsonPropertyName("includeOveralPosition")]
        public bool IncludeOverallPosition { get; set; }

        [JsonPropertyName("customerStrategyRefs")]
        public List<string> CustomerStrategyRefs { get; set; } = null!;
    }
}

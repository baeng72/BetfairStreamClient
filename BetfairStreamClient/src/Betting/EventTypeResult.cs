using System.Text.Json.Serialization;

namespace BetfairStreamClient.Betting
{
    public class EventTypeResult
    {
        [JsonPropertyName("eventType")]
        public EventType EventType { get; set; } = null!;

        [JsonPropertyName("marketCount")]
        public int MarketCount { get; set; }
    }
}

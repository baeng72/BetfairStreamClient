using System.Text.Json.Serialization;

namespace BFBot.Betting
{
    public class EventTypeResult
    {
        [JsonPropertyName("eventType")]
        public EventType EventType { get; set; }

        [JsonPropertyName("marketCount")]
        public int MarketCount { get; set; }
    }
}

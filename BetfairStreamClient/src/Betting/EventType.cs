using System.Text.Json.Serialization;

namespace BetfairStreamClient.Betting
{
    public sealed class EventType
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = "";

        [JsonPropertyName("name")]
        public string Name { get; set; } = "";
    }


}

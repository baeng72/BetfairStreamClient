using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace BetfairStreamClient.ExchangeStream
{
    public class RequestMessage
    {

        [JsonPropertyName("op")]
        public string Op { get; set; } = null!;

        [JsonPropertyName("id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? Id { get; set; }
        [JsonPropertyName("segmentationEnabled")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? SegmentationEnabled { get; set; }

        [JsonPropertyName("clk")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Clk { get; set; }

        [JsonPropertyName("heartbeatMs")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public long? HeartbeatMs { get; set; }

        [JsonPropertyName("initialClk")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? InitialClick { get; set; }

        [JsonPropertyName("conflateMs")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public long? ConflateMs { get; set; }

    }
}

using System.Text.Json.Serialization;


namespace StreamTest.Stream
{
    public class MarketChange
    {
        [JsonPropertyName("rc")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<RunnerChange> RunnerChanges { get; set; }

        [JsonPropertyName("img")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? Image { get; set; }

        [JsonPropertyName("tv")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? TotalAmountMatched { get; set; }

        [JsonPropertyName("con")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? Conflated { get; set; }

        [JsonPropertyName("marketDefinition")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public MarketDefinition MarketDefinition { get; set; }

        [JsonPropertyName("id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string MarketId { get; set; }
    }
}

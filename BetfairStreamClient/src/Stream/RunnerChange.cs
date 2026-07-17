using System.Text.Json.Serialization;

namespace BetfairStreamClient.Stream
{
    public class RunnerChange
    {
        [JsonPropertyName("tv")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? TotalVolume { get; set; }

        [JsonPropertyName("batb")]
        [JsonConverter(typeof(DeltaConverter))]
        public LevelDelta[] BestAvailableToBack { get; set; } = null!;

        [JsonPropertyName("spb")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<List<double?>> StartingPriceBack { get; set; } = null!;

        [JsonPropertyName("bdatl")]
        [JsonConverter(typeof(DeltaConverter))]
        public LevelDelta[] BestDisplayAvailableToLay { get; set; } = null!;

        [JsonPropertyName("trd")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<List<double?>> TradedPriceVolume { get; set; } = null!;

        [JsonPropertyName("spf")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? StartingPriceFar { get; set; }

        [JsonPropertyName("ltp")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? LastTradedPrice { get; set; }

        [JsonPropertyName("atb")]
        [JsonConverter(typeof(DeltaConverter))]
        public LevelDelta[] AvailableToBack { get; set; } = null!;

        [JsonPropertyName("spl")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<List<double?>> StartingPriceLay { get; set; } = null!;

        [JsonPropertyName("spn")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? StartingPriceNear { get; set; }

        [JsonPropertyName("atl")]
        [JsonConverter(typeof(DeltaConverter))]
        public LevelDelta[] AvailableToLay { get; set; } = null!;

        [JsonPropertyName("batl")]
        [JsonConverter(typeof(DeltaConverter))]
        public LevelDelta[] BestAvailableToLay { get; set; } = null!;

        [JsonPropertyName("id")]        
        public long SelectionId { get; set; }

        [JsonPropertyName("hc")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? Handicap { get; set; }

        [JsonPropertyName("bdatb")]
        [JsonConverter(typeof(DeltaConverter))]
        public LevelDelta[] BestDisplayAvailableToBack { get; set; } = null!;


    }
}

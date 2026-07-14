using System.Text.Json.Serialization;

namespace BetfairStreamClient.Stream
{
    public class RunnerChange
    {
        [JsonPropertyName("tv")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? TotalVolume { get; set; }

        [JsonPropertyName("batb")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<List<double?>> BestAvailableToBack { get; set; } = null!;

        [JsonPropertyName("spb")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<List<double?>> StartingPriceBack { get; set; } = null!;

        [JsonPropertyName("bdatl")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<List<double?>> BestDisplayAvailableToLay { get; set; } = null!;

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
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<List<double?>> AvailableToBack { get; set; } = null!;

        [JsonPropertyName("spl")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<List<double?>> StartingPriceLay { get; set; } = null!;

        [JsonPropertyName("spn")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? StartingPriceNear { get; set; }

        [JsonPropertyName("atl")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<List<double?>> AvailableToLay { get; set; } = null!;

        [JsonPropertyName("batl")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<List<double?>> BestAvailableToLay { get; set; } = null!;

        [JsonPropertyName("id")]        
        public long SelectionId { get; set; }

        [JsonPropertyName("hc")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? Handicap { get; set; }

        [JsonPropertyName("bdatb")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<List<double?>> BestDisplayAvailableToBack { get; set; } = null!;


    }
}

using System.Text.Json.Serialization;

namespace StreamTest.Stream
{
    public class RunnerChange
    {
        [JsonPropertyName("tv")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? TotalVolume { get; set; }

        [JsonPropertyName("batb")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<List<double?>> BestAvailableToBack { get; set; }

        [JsonPropertyName("spb")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<List<double?>> StartingPriceBack { get; set; }

        [JsonPropertyName("bdatl")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<List<double?>> BestDisplayAvailableToLay { get; set; }

        [JsonPropertyName("trd")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<List<double?>> TradedPriceVolume { get; set; }

        [JsonPropertyName("spf")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? StartingPriceFar { get; set; }

        [JsonPropertyName("ltp")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? LastTradedPrice { get; set; }

        [JsonPropertyName("atb")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<List<double?>> AvailableToBack { get; set; }

        [JsonPropertyName("spl")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<List<double?>> StartingPriceLay { get; set; }

        [JsonPropertyName("spn")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? StartingPriceNear { get; set; }

        [JsonPropertyName("atl")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<List<double?>> AvailableToLay { get; set; }

        [JsonPropertyName("batl")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<List<double?>> BestAvailableToLay { get; set; }

        [JsonPropertyName("id")]        
        public long SelectionId { get; set; }

        [JsonPropertyName("hc")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? Handicap { get; set; }

        [JsonPropertyName("bdatb")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<List<double?>> BestDisplayAvailableToBack { get; set; }


    }
}

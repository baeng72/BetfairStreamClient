using System.Text.Json.Serialization;

namespace BetfairStreamClient.ExchangeStream
{
    public class MarketRunnerChange
    {
        private const int MaxLevels = 20;

        [JsonPropertyName("tv")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? TotalVolume { get; set; }

        [JsonPropertyName("batb")]
        [JsonConverter(typeof(DeltaConverter))]
        public LevelDelta[] BestAvailableToBack { get; set; } = new LevelDelta[MaxLevels];
        public int BestAvailableToBackCount { get; set; } = 0;

        [JsonPropertyName("spb")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public PriceSize[] StartingPriceBack { get; set; } = new PriceSize[MaxLevels];
        public int StartingPriceBackCount { get; set; } = 0;

        [JsonPropertyName("bdatl")]
        [JsonConverter(typeof(DeltaConverter))]
        public LevelDelta[] BestDisplayAvailableToLay { get; set; } = new LevelDelta[MaxLevels];
        public int BestDisplayAvailableToLayCount { get; set; } = 0;

        [JsonPropertyName("trd")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public PriceSize[] TradedPriceVolume { get; set; } = new PriceSize[MaxLevels];
        public int TradedPriceVolumeCount { get; set; } = 0;

        [JsonPropertyName("spf")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? StartingPriceFar { get; set; }

        [JsonPropertyName("ltp")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? LastTradedPrice { get; set; }

        [JsonPropertyName("atb")]
        [JsonConverter(typeof(DeltaConverter))]
        public PriceSize[] AvailableToBack { get; set; } = new PriceSize[MaxLevels];
        public int AvailableToBackCount { get; set; } = 0;

        [JsonPropertyName("spl")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public PriceSize[] StartingPriceLay { get; set; } = new PriceSize[MaxLevels];

        [JsonPropertyName("spn")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? StartingPriceNear { get; set; }

        [JsonPropertyName("atl")]
        [JsonConverter(typeof(DeltaConverter))]
        public PriceSize[] AvailableToLay { get; set; } = new PriceSize[MaxLevels];
        public int AvailableToLayCount { get; set; } = 0;

        [JsonPropertyName("batl")]
        [JsonConverter(typeof(DeltaConverter))]
        public LevelDelta[] BestAvailableToLay { get; set; } = new LevelDelta[MaxLevels];
        public int BestAvailableToLayCount { get; set; } = 0;

        [JsonPropertyName("id")]        
        public long SelectionId { get; set; }

        [JsonPropertyName("hc")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? Handicap { get; set; }

        [JsonPropertyName("bdatb")]
        [JsonConverter(typeof(DeltaConverter))]
        public LevelDelta[] BestDisplayAvailableToBack { get; set; } = new LevelDelta[MaxLevels];
        public int BestDisplayAvailableToBackCount { get; set; } = 0;


    }
}

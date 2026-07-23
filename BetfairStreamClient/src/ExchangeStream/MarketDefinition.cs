using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace BetfairStreamClient.ExchangeStream
{
    public class MarketDefinition
    {
        /// <summary>
        ///     Gets or Sets BettingType
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum BettingTypeEnum
        {
            [JsonStringEnumMemberName("ODDS")]
            Odds,

            [JsonStringEnumMemberName("LINE")]
            Line,

            [JsonStringEnumMemberName("RANGE")]
            Range,

            [JsonStringEnumMemberName("ASIAN_HANDICAP_DOUBLE_LINE")]
            AsianHandicapDoubleLine,

            [JsonStringEnumMemberName("ASIAN_HANDICAP_SINGLE_LINE")]
            AsianHandicapSingleLine
        }

        /// <summary>
        ///     Gets or Sets Status
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum StatusEnum
        {
            [JsonStringEnumMemberName("INACTIVE")]
            Inactive,

            [JsonStringEnumMemberName("OPEN")]
            Open,

            [JsonStringEnumMemberName("SUSPENDED")]
            Suspended,

            [JsonStringEnumMemberName("CLOSED")]
            Closed
        }

        

        [JsonPropertyName("bettingType")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public BettingTypeEnum? BettingType { get; set; }

        [JsonPropertyName("status")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public StatusEnum? Status { get; set; }

        [JsonPropertyName("venue")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Venue { get; set; } = null!;

        [JsonPropertyName("settledTime")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DateTime? SettledTime { get; set; }

        [JsonPropertyName("timezone")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Timezone { get; set; } = null!;

        [JsonPropertyName("eachWayDivisor")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? EachWayDivisor { get; set; }

        [JsonPropertyName("regulators")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string> Regulators { get; set; } = null!;

        [JsonPropertyName("marketType")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string MarketType { get; set; } = null!;

        [JsonPropertyName("marketBaseRate")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? MarketBaseRate { get; set; }

        [JsonPropertyName("numberOfWinners")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? NumberOfWinners { get; set; }

        [JsonPropertyName("countryCode")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string CountryCode { get; set; } = null!;

        [JsonPropertyName("inPlay")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? InPlay { get; set; }

        [JsonPropertyName("betDelay")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? BetDelay { get; set; }

        [JsonPropertyName("bspMarket")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? BspMarket { get; set; }

        [JsonPropertyName("numberOfActiveRunners")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? NumberOfActiveRunners { get; set; }

        [JsonPropertyName("eventId")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string EventId { get; set; } = null!;

        [JsonPropertyName("crossMatching")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? CrossMatching { get; set; }

        [JsonPropertyName("runnersVoidable")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? RunnersVoidable { get; set; }


        [JsonPropertyName("turnInPlayEnabled")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? TurnInPlayEnabled { get; set; }

        [JsonPropertyName("suspendTime")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DateTime? SuspendTime { get; set; }

        [JsonPropertyName("discountAllowed")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? DiscountAllowed { get; set; }

        [JsonPropertyName("persistenceEnabled")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]

        public bool? PersistenceEnabled { get; set; }

        [JsonPropertyName("runners")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<RunnerDefinition> Runners { get; set; } = null!;

        [JsonPropertyName("version")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public long? Version { get; set; }

        [JsonPropertyName("eventTypeId")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string EventTypeId { get; set; } = null!;

        [JsonPropertyName("complete")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? Complete { get; set; }

        [JsonPropertyName("openDate")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DateTime? OpenDate { get; set; }

        [JsonPropertyName("marketTime")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DateTime? MarketTime { get; set; }

        [JsonPropertyName("bspReconciled")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? BspReconciled { get; set; }
    }
}

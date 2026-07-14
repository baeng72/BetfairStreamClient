using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace BetfairStreamClient.Stream
{
    public class RunnerDefinition
    {
        /// <summary>
        ///     Gets or Sets Status
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum StatusEnum
        {
            [JsonStringEnumMemberName("ACTIVE")]
            Active,

            [JsonStringEnumMemberName("WINNER")]
            Winner,

            [JsonStringEnumMemberName("LOSER")]
            Loser,

            [JsonStringEnumMemberName("REMOVED")]
            Removed,

            [JsonStringEnumMemberName("REMOVED_VACANT")]
            RemovedVacant,

            [JsonStringEnumMemberName("HIDDEN")]
            Hidden,

            [JsonStringEnumMemberName("PLACED")]
            Placed
        }


        /// <summary>
        ///     Gets or Sets Status
        /// </summary>
        [JsonPropertyName("status")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public StatusEnum? Status { get; set; }

        /// <summary>
        ///     Gets or Sets SortPriority
        /// </summary>
        [JsonPropertyName("sortPriority")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? SortPriority { get; set; }


        /// <summary>
        ///     Gets or Sets RemovalDate
        /// </summary>
        [JsonPropertyName("removalDate")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DateTime? RemovalDate { get; set; }

        /// <summary>
        ///     Selection Id - the id of the runner (selection)
        /// </summary>
        /// <value>Selection Id - the id of the runner (selection)</value>
        [JsonPropertyName("id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public long? Id { get; set; }

        /// <summary>
        ///     Handicap - the handicap of the runner (selection) (null if not applicable)
        /// </summary>
        /// <value>Handicap - the handicap of the runner (selection) (null if not applicable)</value>
        [JsonPropertyName("hc")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? Handicap { get; set; }


        /// <summary>
        ///     Gets or Sets AdjustmentFactor
        /// </summary>
        [JsonPropertyName("adjustmentFactor")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? AdjustmentFactor { get; set; }

        /// <summary>
        ///     Gets or Sets Bsp
        /// </summary>
        [JsonPropertyName("bsp")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? Bsp { get; set; }
    }
}

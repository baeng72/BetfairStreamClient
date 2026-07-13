
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace StreamTest.Stream
{
    public class OrderRunnerChange
    {
        /// <summary>
        ///     Matched Backs - matched amounts by distinct matched price on the Back side for this runner (selection)
        /// </summary>
        /// <value>Matched Backs - matched amounts by distinct matched price on the Back side for this runner (selection)</value>
        
        [JsonPropertyName("mb")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<List<double?>> MatchedBacks { get; set; }

        /// <summary>
        ///     Unmatched Orders - orders on this runner (selection) that are not fully matched
        /// </summary>
        /// <value>Unmatched Orders - orders on this runner (selection) that are not fully matched</value>
        
        [JsonPropertyName("uo")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<Order> UnmatchedOrders { get; set; }

        /// <summary>
        ///     Selection Id - the id of the runner (selection)
        /// </summary>
        /// <value>Selection Id - the id of the runner (selection)</value>        
        [JsonPropertyName("id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public long? SelectionId { get; set; }

        /// <summary>
        ///     Handicap - the handicap of the runner (selection) (null if not applicable)
        /// </summary>
        /// <value>Handicap - the handicap of the runner (selection) (null if not applicable)</value>        
        [JsonPropertyName("hc")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? Handicap { get; set; }

        /// <summary>
        ///     Gets or Sets FullImage
        /// </summary>        
        [JsonPropertyName("fullImage")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? FullImage { get; set; }

        /// <summary>
        ///     Matched Lays - matched amounts by distinct matched price on the Lay side for this runner (selection)
        /// </summary>
        /// <value>Matched Lays - matched amounts by distinct matched price on the Lay side for this runner (selection)</value>        
        [JsonPropertyName("ml")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<List<double?>> MatchedLays { get; set; }
    }
}

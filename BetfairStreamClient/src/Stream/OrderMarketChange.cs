using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace BetfairStreamClient.Stream
{
    public class OrderMarketChange
    {
                
        /// <summary>
        ///     Gets or Sets AccountId
        /// </summary>        
        [JsonPropertyName("accountId")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public long? AccountId { get; set; }

        /// <summary>
        ///     Order Changes - a list of changes to orders on a selection
        /// </summary>
        /// <value>Order Changes - a list of changes to orders on a selection</value>
        
        [JsonPropertyName("orc")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<OrderRunnerChange> OrderRunnerChanges { get; set; } = null!;

        /// <summary>
        ///     Gets or Sets Closed
        /// </summary>
        [JsonPropertyName("closed")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? Closed { get; set; }


        /// <summary>
        ///     Market Id - the id of the market the order is on
        /// </summary>
        /// <value>Market Id - the id of the market the order is on</value>        
        [JsonPropertyName("id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string MarketId { get; set; } = null!;

        /// <summary>
        ///     Gets or Sets FullImage
        /// </summary>        
        [JsonPropertyName("fullImage")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? FullImage { get; set; }
    }
}

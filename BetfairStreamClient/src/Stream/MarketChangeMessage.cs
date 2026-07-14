using System.Runtime.Serialization;
using System.Text.Json.Serialization;


namespace BetfairStreamClient.Stream
{
    public class MarketChangeMessage
    {
        [JsonPropertyName("id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Id { get; set; }
        /// <summary>
        ///     MarketChanges - the modifications to markets (will be null on a heartbeat
        /// </summary>
        /// <value>MarketChanges - the modifications to markets (will be null on a heartbeat</value>        
        [JsonPropertyName("mc")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<MarketChange> MarketChanges { get; set; } = null!;

        
    }
}

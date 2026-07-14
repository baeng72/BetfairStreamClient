using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace BetfairStreamClient.Stream
{
    public class OrderChangeMessage : ResponseMessage
    {
        

        /// <summary>
        ///     OrderMarketChanges - the modifications to account&#39;s orders (will be null on a heartbeat
        /// </summary>
        /// <value>OrderMarketChanges - the modifications to account&#39;s orders (will be null on a heartbeat</value>        
        [JsonPropertyName("oc")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<OrderMarketChange> OrderMarketChanges { get; set; } = null!;

        

    }
}

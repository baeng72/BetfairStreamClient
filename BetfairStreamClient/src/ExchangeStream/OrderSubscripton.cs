using System.Text.Json.Serialization;


namespace BetfairStreamClient.ExchangeStream
{
    public class OrderSubscripton : RequestMessage
    {
        
        [JsonPropertyName("orderFilter")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OrderFilter? OrderFilter { get; set; }
        
    }
}

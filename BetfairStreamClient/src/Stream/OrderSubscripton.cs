using System.Text.Json.Serialization;


namespace BetfairStreamClient.Stream
{
    public class OrderSubscripton : RequestMessage
    {
        
        [JsonPropertyName("orderFilter")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OrderFilter? OrderFilter { get; set; }
        
    }
}

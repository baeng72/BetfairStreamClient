using System.Net.NetworkInformation;
using System.Text.Json.Serialization;

namespace BFBot.Betting
{
    public sealed class PlaceInstruction
    {
        public PlaceInstruction(long selectionId, Side side, LimitOrder limitOrder, OrderType orderType = OrderType.LIMIT, string customerOrderRef="")
        {
            SelectionId = selectionId;
            Side = side;
            LimitOrder = limitOrder;
            OrderType = orderType;
            CustomerOrderRef = customerOrderRef;
        }

        [JsonPropertyName("selectionId")]
        public long SelectionId { get; set; }

        [JsonPropertyName("side")]
        public Side Side { get; set; }

        [JsonPropertyName("orderType")]
        public OrderType OrderType { get; set; }

        [JsonPropertyName("limitOrder")]
        public LimitOrder? LimitOrder { get; set; }

        [JsonPropertyName("handicap")]
        public double? Handicap { get; set; }

        [JsonPropertyName("customerOrderRef")]
        public string? CustomerOrderRef { get; set; }
    }

}

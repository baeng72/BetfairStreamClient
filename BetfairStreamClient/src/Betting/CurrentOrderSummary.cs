using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BetfairStreamClient.Betting
{
    public sealed class CurrentOrderSummary
    {
        [JsonPropertyName("betId")]
        public string BetId { get; set; } = "";

        [JsonPropertyName("marketId")]
        public string MarketId { get; set; } = "";

        [JsonPropertyName("selectionId")]
        public long SelectionId { get; set; }

        [JsonPropertyName("side")]
        public Side Side { get; set; }

        [JsonPropertyName("status")]
        public OrderStatus Status { get; set; } // EXECUTION_COMPLETE | EXECUTABLE

        [JsonPropertyName("priceSize")]
        public PriceSize? PriceSize { get; set; }

        [JsonPropertyName("sizeMatched")]
        public double SizeMatched { get; set; }

        [JsonPropertyName("sizeRemaining")]
        public double SizeRemaining { get; set; }

        [JsonPropertyName("placedDate")]
        public DateTimeOffset? PlacedDate { get; set; }
    }

}

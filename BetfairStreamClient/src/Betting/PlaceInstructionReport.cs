using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BetfairStreamClient.Betting
{
    public sealed class PlaceInstructionReport
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = ""; // SUCCESS | FAILURE | TIMEOUT

        [JsonPropertyName("errorCode")]
        public string? ErrorCode { get; set; }

        [JsonPropertyName("betId")]
        public string? BetId { get; set; }

        [JsonPropertyName("placedDate")]
        public DateTimeOffset? PlacedDate { get; set; }

        [JsonPropertyName("averagePriceMatched")]
        public double? AveragePriceMatched { get; set; }

        [JsonPropertyName("sizeMatched")]
        public double? SizeMatched { get; set; }
    }

}

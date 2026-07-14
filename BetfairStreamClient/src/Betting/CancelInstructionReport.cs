using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BetfairStreamClient.Betting
{
    public sealed class CancelInstructionReport
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = "";

        [JsonPropertyName("errorCode")]
        public string? ErrorCode { get; set; }

        [JsonPropertyName("betId")]
        public string? BetId { get; set; }

        [JsonPropertyName("sizeCancelled")]
        public double SizeCancelled { get; set; }

        [JsonPropertyName("cancelledDate")]
        public DateTimeOffset? CancelledDate { get; set; }
    }


}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BFBot.Betting
{
    public sealed class PlaceExecutionReport
    {
        [JsonPropertyName("customerRef")]
        public string? CustomerRef { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = ""; // SUCCESS | FAILURE | PROCESSED_WITH_ERRORS

        [JsonPropertyName("errorCode")]
        public string? ErrorCode { get; set; }

        [JsonPropertyName("marketId")]
        public string MarketId { get; set; } = "";

        [JsonPropertyName("instructionReports")]
        public List<PlaceInstructionReport>? InstructionReports { get; set; }
    }


}

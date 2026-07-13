using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BFBot.Betting
{
    public sealed class CancelInstruction
    {
        public CancelInstruction(string betId, double? sizeReduction = null)
        {
            BetId = betId;
            SizeReduction = sizeReduction;
        }

        [JsonPropertyName("betId")]
        public string BetId { get; set; }

        /// <summary>Leave null to cancel the full remaining size of the bet.</summary>
        [JsonPropertyName("sizeReduction")]
        public double? SizeReduction { get; set; }
    }

}

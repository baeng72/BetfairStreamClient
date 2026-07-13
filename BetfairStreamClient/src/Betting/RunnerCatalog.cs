using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BFBot.Betting
{

    public sealed class RunnerCatalog
    {
        [JsonPropertyName("selectionId")]
        public long SelectionId { get; set; }

        [JsonPropertyName("runnerName")]
        public string RunnerName { get; set; } = "";

        [JsonPropertyName("handicap")]
        public double Handicap { get; set; }

        [JsonPropertyName("sortPriority")]
        public int SortPriority { get; set; }
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BetfairStreamClient.InPlayerService.Model
{
    public class HomeAway
    {
        
        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        [JsonPropertyName("score")]
        
        public string Score { get; set; } = null!;

        [JsonPropertyName("halfTimeScore")]
        
        public string HalfTimeScore { get; set; } = null!;

        [JsonPropertyName("fullTimeScore")]
        
        public string FullTimeScore { get; set; }   = null!;

        [JsonPropertyName("penaltiesScore")]
        
        public string PenaltiesScore { get; set; } = null!;

        [JsonPropertyName("penaltiesSequence")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public List<int> PenaltiesSequence { get; set; } = null!;

        [JsonPropertyName("games")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int Games { get; set; }

        [JsonPropertyName("sets")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int Sets { get; set; }

        [JsonPropertyName("gameSequence")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public List<int> GameSequence { get; set; } = null!;

        [JsonPropertyName("isServing")]
        public bool IsServing { get; set; }

        [JsonPropertyName("highlight")]
        public bool Highlight { get; set; }

        [JsonPropertyName("serviceBreaks")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int ServiceBreaks { get; set; }
    }
}

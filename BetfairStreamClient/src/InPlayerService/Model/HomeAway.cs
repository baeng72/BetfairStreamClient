using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StreamTest.InPlayerService.Model
{
    public class HomeAway
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("score")]
        
        public string Score { get; set; }

        [JsonPropertyName("halfTimeScore")]
        
        public string HalfimeScore { get; set; }

        [JsonPropertyName("fullTimeScore")]
        
        public string FullTimeScore { get; set; }

        [JsonPropertyName("penaltiesScore")]
        
        public string PenaltiesScore { get; set; }

        [JsonPropertyName("penaltiesSequence")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public List<int> PenaltiesSequence { get; set; }

        [JsonPropertyName("games")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int Games { get; set; }

        [JsonPropertyName("sets")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int Sets { get; set; }

        [JsonPropertyName("gameSequence")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public List<int> GameSequence { get; set; }

        [JsonPropertyName("isServing")]
        public bool IsServing { get; set; }

        [JsonPropertyName("highlight")]
        public bool Highlight { get; set; }

        [JsonPropertyName("serviceBreaks")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int ServiceBreaks { get; set; }
    }
}

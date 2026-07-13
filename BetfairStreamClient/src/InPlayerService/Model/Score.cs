using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StreamTest.InPlayerService.Model
{
    public partial class Score
    {
        [JsonPropertyName("home")]
        public HomeAway Home { get; set; }

        [JsonPropertyName("away")]
        public HomeAway Away { get; set; }
    }
}

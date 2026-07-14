using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BetfairStreamClient.InPlayerService.Model
{
    public class FullTimeElapsed
    {
        [JsonPropertyName("hour")]
        public int Hour { get; set; }

        [JsonPropertyName("min")]
        public int Minute { get; set; }

        [JsonPropertyName("sec")]
        public int Second { get; set; }
    }
}

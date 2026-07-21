using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BetfairStreamClient.src.Account
{
    public class VenueResult
    {
        [JsonPropertyName("venue")]
        public string Venue { get; set; }

        [JsonPropertyName("marketCount")]
        public int MarketCount { get; set; }
    }
}

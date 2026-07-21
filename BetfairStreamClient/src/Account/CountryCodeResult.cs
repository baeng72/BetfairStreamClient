using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BetfairStreamClient.src.Account
{
    public class CountryCodeResult
    {
        [JsonPropertyName("countryCode")]
        public string CountryCode { get; set; }

        [JsonPropertyName("marketCount")]
        public int MarketCount { get; set; }
    }
}

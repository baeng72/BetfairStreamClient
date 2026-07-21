using BetfairStreamClient.Betting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BetfairStreamClient.src.Account
{
    public class TimeRangeResult
    {
        [JsonPropertyName("timeRange")]
        public TimeRange TimeRange { get; set; }

        [JsonPropertyName("marketCount")]
        public int MarketCount { get; set; }
    }
}

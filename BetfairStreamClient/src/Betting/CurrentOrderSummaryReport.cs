using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BetfairStreamClient.Betting
{
    public sealed class CurrentOrderSummaryReport
    {
        [JsonPropertyName("currentOrders")]
        public List<CurrentOrderSummary>? CurrentOrders { get; set; }

        [JsonPropertyName("moreAvailable")]
        public bool MoreAvailable { get; set; }
    }


}

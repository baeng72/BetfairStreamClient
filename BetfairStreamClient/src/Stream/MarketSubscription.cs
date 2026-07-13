using BFBot.Betting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StreamTest.Stream
{
    public class MarketSubscription : RequestMessage
    {
        /// <summary>
        ///     Gets or Sets MarketFilter
        /// </summary>        
        [JsonPropertyName("marketFilter")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public MarketFilter MarketFilter { get; set; }

   
        /// <summary>
        ///     Gets or Sets MarketDataFilter
        /// </summary>
        [DataMember(Name = "marketDataFilter", EmitDefaultValue = false)]
        [JsonPropertyName("marketDataFilter")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public MarketDataFilter MarketDataFilter { get; set; }
    }
}

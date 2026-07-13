using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StreamTest.Stream
{
    public class MarketDataFilter
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum FieldsEnum
        {
            [JsonStringEnumMemberName("EX_BEST_OFFERS_DISP")]
            ExBestOffersDisp,

            [JsonStringEnumMemberName("EX_BEST_OFFERS")]
            ExBestOffers,

            [JsonStringEnumMemberName("EX_ALL_OFFERS")]
            ExAllOffers,

            [JsonStringEnumMemberName("EX_TRADED")]
            ExTraded,

            [JsonStringEnumMemberName("EX_TRADED_VOL")]
            ExTradedVol,

            [JsonStringEnumMemberName("EX_LTP")]
            ExLtp,

            [JsonStringEnumMemberName("EX_MARKET_DEF")]
            ExMarketDef,

            [JsonStringEnumMemberName("SP_TRADED")]
            SpTraded,

            [JsonStringEnumMemberName("SP_PROJECTED")]
            SpProjected
        }


        /// <summary>
        ///     Gets or Sets LadderLevels
        /// </summary>        
        [JsonPropertyName("ladderLevels")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? LadderLevels { get; set; }

        /// <summary>
        ///     Gets or Sets Fields
        /// </summary>        
        [JsonPropertyName("fields")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<FieldsEnum?> Fields { get; set; }
    }
}

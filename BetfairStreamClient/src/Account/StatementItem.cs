using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BetfairStreamClient.src.Account
{
    public class StatementItem
    {
        [JsonPropertyName("refId")]
        public string RefId { get; set; }

        [JsonPropertyName("itemDate")]
        public DateTime ItemDate { get; set; }

        [JsonPropertyName("amount")]
        public double Amount { get; set; }

        [JsonPropertyName("balance")]
        public double Balance { get; set; }

        [JsonPropertyName("itemClass")]
        public ItemClass ItemClass { get; set; }

        [JsonPropertyName("itemClassData")]
        public IDictionary<string, string> ItemClassData { get; set; }

        [JsonPropertyName("legacyData")]
        public StatementLegacyData LegacyData { get; set; }
    }
}

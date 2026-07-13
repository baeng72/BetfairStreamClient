using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BFBot.Betting
{

    public sealed class PriceSize
    {
        [JsonPropertyName("price")]
        public double Price { get; set; }

        [JsonPropertyName("size")]
        public double Size { get; set; }
    }

}

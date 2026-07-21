using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BetfairStreamClient.src.Nav
{
    /*A ROOT group node has one or many EVENT_TYPE nodes

An EVENT_TYPE node has zero, one or many GROUP nodes

An EVENT_TYPE node has zero, one or many EVENT nodes

A Horse Racing EVENT_TYPE node has zero, one or many RACE nodes

A RACE node has one or many MARKET nodes

A GROUP node has zero, one or many EVENT nodes

A GROUP node has zero, one or many GROUP nodes

An EVENT node has zero, one or many MARKET nodes

An EVENT node has zero, one or many GROUP nodes

An EVENT node has zero, one or many EVENT nodes*/
    public class MenuNode
    {



        [JsonPropertyName("children")]
        public List<MenuNode> children { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("type")]
        public NodeType Type { get; set; }

        //Event specific fields
        [JsonPropertyName("countryCode")]
        public string countryCode { get; set; }

        //RACE specific fields
        [JsonPropertyName("startTime")]
        public DateTime? startTime { get; set; }

        [JsonPropertyName("venue")]
        public string venue { get; set; }

        [JsonPropertyName("raceNumber")]
        public string raceNumber { get; set; }

        //Market specific fields
        [JsonPropertyName("exchangeId")]
        public string exchangeId { get; set; }

        [JsonPropertyName("marketStartTime")]
        public DateTime? marketStartTime { get; set; }

        [JsonPropertyName("marketType")]
        public string marketType { get; set; }

        [JsonPropertyName("numberOfWinners")]
        public string numberOfWinners { get; set; }


    }
}

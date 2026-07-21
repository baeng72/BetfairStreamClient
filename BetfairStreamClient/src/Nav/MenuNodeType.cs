using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BetfairStreamClient.src.Nav
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum NodeType { UNKNOWN, GROUP, EVENT_TYPE, EVENT, RACE, MARKET };
}

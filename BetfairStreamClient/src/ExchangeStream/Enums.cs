using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BetfairStreamClient.ExchangeStream
{
    /// <summary>
    ///     Side - the side of the order
    /// </summary>
    /// <value>Side - the side of the order</value>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum SideEnum
    {
        [JsonStringEnumMemberName("B")]
        Back,

        [JsonStringEnumMemberName("L")]
        Lay
    }

    /// <summary>
    ///     Persistence Type - whether the order will persist at in play or not (L = LAPSE, P = PERSIST, MOC = Market On Close)
    /// </summary>
    /// <value>Persistence Type - whether the order will persist at in play or not (L = LAPSE, P = PERSIST, MOC = Market On Close)</value>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum PtEnum
    {
        [JsonStringEnumMemberName("L")]
        LAPSE,

        [JsonStringEnumMemberName("P")]
        PERSIST,

        [JsonStringEnumMemberName("MOC")]
        MARKET_ON_CHANGE
    }

    /// <summary>
    ///     Order Type - the type of the order (L = LIMIT, MOC = MARKET_ON_CLOSE, LOC = LIMIT_ON_CLOSE)
    /// </summary>
    /// <value>Order Type - the type of the order (L = LIMIT, MOC = MARKET_ON_CLOSE, LOC = LIMIT_ON_CLOSE)</value>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum OtEnum
    {
        [JsonStringEnumMemberName("L")]
        LIMIT,

        [JsonStringEnumMemberName("LOC")]
        LIMIT_ON_CLOSE,

        [JsonStringEnumMemberName("MOC")]
        MARKET_ON_CLOSE
    }

    /// <summary>
    ///     Status - the status of the order (E = EXECUTABLE, EC = EXECUTION_COMPLETE)
    /// </summary>
    /// <value>Status - the status of the order (E = EXECUTABLE, EC = EXECUTION_COMPLETE)</value>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum StatusEnum
    {
        [JsonStringEnumMemberName("E")]
        EXECUTABLE,

        [JsonStringEnumMemberName("EC")]
        EXECUTION_COMPLETE
    }
}


using System.Text.Json.Serialization;

namespace BFBot.Betting
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Side
    {
        BACK,
        LAY
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum OrderType
    {
        LIMIT,
        LIMIT_ON_CLOSE,
        MARKET_ON_CLOSE
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum PersistenceType
    {
        LAPSE,
        PERSIST,
        MARKET_ON_CLOSE
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum TimeInForce
    {
        FILL_OR_KILL,
        FILL_OR_KILL_IF_UNMATCHED
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum MarketProjection
    {
        COMPETITION,
        EVENT,
        EVENT_TYPE,
        MARKET_START_TIME,
        MARKET_DESCRIPTION,
        RUNNER_DESCRIPTION,
        RUNNER_METADATA
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum MarketSort
    {
        MINIMUM_TRADED,
        MAXIMUM_TRADED,
        MINIMUM_AVAILABLE,
        MAXIMUM_AVAILABLE,
        FIRST_TO_START,
        LAST_TO_START
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum OrderStatus
    {
        EXECUTION_COMPLETE,
        EXECUTABLE
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum PriceData
    {
        SP_AVAILABLE, SP_TRADED,
        EX_BEST_OFFERS, EX_ALL_OFFERS, EX_TRADED,
    }
}
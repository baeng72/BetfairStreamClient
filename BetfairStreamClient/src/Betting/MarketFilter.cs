using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace BFBot.Betting
{
    /// <summary>Mirrors Betfair's TimeRange type (ISO-8601 strings, e.g. "2026-07-04T00:00:00Z").</summary>
    public sealed class TimeRange
    {
        [JsonPropertyName("from")]
        public DateTime From { get; set; }

        [JsonPropertyName("to")]
        public DateTime To { get; set; }
    }

    /// <summary>Mirrors Betfair's MarketFilter type. All fields optional; only
    /// non-null values are serialized (see BetfairJson.Options).</summary>
    public sealed class MarketFilter
    {
        [JsonPropertyName("textQuery")]
        public string? TextQuery { get; set; }

        [JsonPropertyName("eventTypeIds")]
        public List<string>? EventTypeIds { get; set; }

        [JsonPropertyName("eventIds")]
        public List<string>? EventIds { get; set; }

        [JsonPropertyName("competitionIds")]
        public List<string>? CompetitionIds { get; set; }

        [JsonPropertyName("marketIds")]
        public List<string>? MarketIds { get; set; }

        [JsonPropertyName("marketCountries")]
        public List<string>? MarketCountries { get; set; }

        [JsonPropertyName("marketTypeCodes")]
        public List<string>? MarketTypeCodes { get; set; }

        [JsonPropertyName("marketStartTime")]
        public TimeRange? MarketStartTime { get; set; }

        [JsonPropertyName("marketBettingTypes")]
        public List<string>? MarketBettingTypes { get; set; }

        [JsonPropertyName("inPlayOnly")]
        public bool? InPlayOnly { get; set; }

        [JsonPropertyName("turnInPlayEnabled")]
        public bool? TurnInPlayEnabled { get; set; }
    }
}

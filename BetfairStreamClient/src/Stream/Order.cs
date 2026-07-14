using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace BetfairStreamClient.Stream
{
    public class Order
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

        /// <summary>
        ///     Side - the side of the order
        /// </summary>
        /// <value>Side - the side of the order</value>        
        [JsonPropertyName("side")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public SideEnum? Side { get; set; }

        /// <summary>
        ///     Persistence Type - whether the order will persist at in play or not (L = LAPSE, P = PERSIST, MOC = Market On Close)
        /// </summary>
        /// <value>Persistence Type - whether the order will persist at in play or not (L = LAPSE, P = PERSIST, MOC = Market On Close)</value>        
        [JsonPropertyName("pt")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public PtEnum? Pt { get; set; }

        /// <summary>
        ///     Order Type - the type of the order (L = LIMIT, MOC = MARKET_ON_CLOSE, LOC = LIMIT_ON_CLOSE)
        /// </summary>
        /// <value>Order Type - the type of the order (L = LIMIT, MOC = MARKET_ON_CLOSE, LOC = LIMIT_ON_CLOSE)</value>
        [JsonPropertyName("ot")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OtEnum? Ot { get; set; }

        /// <summary>
        ///     Status - the status of the order (E = EXECUTABLE, EC = EXECUTION_COMPLETE)
        /// </summary>
        /// <value>Status - the status of the order (E = EXECUTABLE, EC = EXECUTION_COMPLETE)</value>        
        [JsonPropertyName("status")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public StatusEnum? Status { get; set; }

        /// <summary>
        ///     Size Voided - the amount of the order that has been voided
        /// </summary>
        /// <value>Size Voided - the amount of the order that has been voided</value>        
        [JsonPropertyName("sv")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? SizeVoided { get; set; }

        /// <summary>
        ///     Price - the original placed price of the order
        /// </summary>
        /// <value>Price - the original placed price of the order</value>        
        [JsonPropertyName("p")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? Price { get; set; }


        /// <summary>
        ///     Size Cancelled - the amount of the order that has been cancelled
        /// </summary>
        /// <value>Size Cancelled - the amount of the order that has been cancelled</value>        
        [JsonPropertyName("sc")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? SizeCancelled { get; set; }

        /// <summary>
        ///     Regulator Code - the regulator of the order
        /// </summary>
        /// <value>Regulator Code - the regulator of the order</value>        
        [JsonPropertyName("rc")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string RegulatorCode { get; set; } = null!;

        /// <summary>
        ///     Size - the original placed size of the order
        /// </summary>
        /// <value>Size - the original placed size of the order</value>        
        [JsonPropertyName("s")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? Size { get; set; }

        /// <summary>
        ///     Placed Date - the date the order was placed
        /// </summary>
        /// <value>Placed Date - the date the order was placed</value>
        [JsonPropertyName("pd")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public long? PlacedDate { get; set; }

        /// <summary>
        ///     Regulator Auth Code - the auth code returned by the regulator
        /// </summary>
        /// <value>Regulator Auth Code - the auth code returned by the regulator</value>        
        [JsonPropertyName("rac")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string RegulatorAuthCode { get; set; } = null!;

        /// <summary>
        ///     Matched Date - the date the order was matched (null if the order is not matched)
        /// </summary>
        /// <value>Matched Date - the date the order was matched (null if the order is not matched)</value>        
        [JsonPropertyName("md")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public long? MatchedDate { get; set; }

        /// <summary>
        ///     Size Lapsed - the amount of the order that has been lapsed
        /// </summary>
        /// <value>Size Lapsed - the amount of the order that has been lapsed</value>
        [JsonPropertyName("sl")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? SizeLapsed { get; set; }

        /// <summary>
        ///     Average Price Matched - the average price the order was matched at (null if the order is not matched
        /// </summary>
        /// <value>Average Price Matched - the average price the order was matched at (null if the order is not matched</value>        
        [JsonPropertyName("avp")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? AveratePriceMethod { get; set; }


        /// <summary>
        ///     Size Matched - the amount of the order that has been matched
        /// </summary>
        /// <value>Size Matched - the amount of the order that has been matched</value>        
        [JsonPropertyName("sm")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? SizeMatched { get; set; }

        /// <summary>
        ///     Bet Id - the id of the order
        /// </summary>
        /// <value>Bet Id - the id of the order</value>        
        [JsonPropertyName("id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string BetId { get; set; } = null!;


        /// <summary>
        ///     BSP Liability - the BSP liability of the order (null if the order is not a BSP order)
        /// </summary>
        /// <value>BSP Liability - the BSP liability of the order (null if the order is not a BSP order)</value>
        [JsonPropertyName("bsp")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? BSPLiability { get; set; }

        /// <summary>
        ///     Size Remaining - the amount of the order that is remaining unmatched
        /// </summary>
        /// <value>Size Remaining - the amount of the order that is remaining unmatched</value>        
        [JsonPropertyName("sr")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? SizeRemaining { get; set; }

        /// <summary>
        ///     Cancelled Date - the date the order was cancelled (null if the order is not cancelled)
        /// </summary>
        /// <value>Cancelled Date - the date the order was cancelled (null if the order is not cancelled)</value>        
        [JsonPropertyName("cd")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public long? CancelledDate { get; set; }


        /// <summary>
        ///  Reference Order
        /// </summary>
        
        [JsonPropertyName("rfo")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string ReferenceOrder { get; set; } = null!;

        
        [JsonPropertyName("rfs")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string ReferenceStrategy { get; set; } = null!;


    }
}

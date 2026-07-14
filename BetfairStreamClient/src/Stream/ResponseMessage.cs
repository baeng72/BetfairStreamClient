using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BetfairStreamClient.Stream
{
    public class ResponseMessage 
    {
        

        /// <summary>
        ///     Change Type - set to indicate the type of change - if null this is a delta)
        /// </summary>
        /// <value>Change Type - set to indicate the type of change - if null this is a delta)</value>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum CtEnum
        {
            [JsonStringEnumMemberName("SUB_IMAGE")]
            SubImage,

            [EnumMember(Value = "RESUB_DELTA")]
            ResubDelta,

            [JsonStringEnumMemberName("HEARTBEAT")]
            Heartbeat
        }

        /// <summary>
        ///     Segment Type - if the change is split into multiple segments, this denotes the beginning and end of a change, and segments in between. Will be null if data is not segmented
        /// </summary>
        /// <value>Segment Type - if the change is split into multiple segments, this denotes the beginning and end of a change, and segments in between. Will be null if data is not segmented</value>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum SegmentTypeEnum
        {
            [JsonStringEnumMemberName("SEG_START")]
            SegStart,

            [JsonStringEnumMemberName("SEG")]
            Seg,

            [JsonStringEnumMemberName("SEG_END")]
            SegEnd
        }
        [JsonPropertyName("op")]
        public string Op { get; set; } = null!;

        [JsonPropertyName("id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? Id { get; set; }
        /// <summary>
        ///     Change Type - set to indicate the type of change - if null this is a delta)
        /// </summary>
        /// <value>Change Type - set to indicate the type of change - if null this is a delta)</value>        
        [JsonPropertyName("ct")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public CtEnum? ChangeType { get; set; }

        /// <summary>
        ///     Segment Type - if the change is split into multiple segments, this denotes the beginning and end of a change, and segments in between. Will be null if data is not segmented
        /// </summary>
        /// <value>Segment Type - if the change is split into multiple segments, this denotes the beginning and end of a change, and segments in between. Will be null if data is not segmented</value>        
        [JsonPropertyName("segmentType")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public SegmentTypeEnum? SegmentType { get; set; }

        /// <summary>
        ///     Token value (non-null) should be stored and passed in a MarketSubscriptionMessage to resume subscription (in case of disconnect)
        /// </summary>
        /// <value>Token value (non-null) should be stored and passed in a MarketSubscriptionMessage to resume subscription (in case of disconnect)</value>        
        [JsonPropertyName("clk")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Click { get; set; } = null!;

        /// <summary>
        ///     Heartbeat Milliseconds - the heartbeat rate (may differ from requested: bounds are 500 to 30000)
        /// </summary>
        /// <value>Heartbeat Milliseconds - the heartbeat rate (may differ from requested: bounds are 500 to 30000)</value>        
        [JsonPropertyName("heartbeatMs")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public long? HeartbeatMs { get; set; }

        /// <summary>
        ///     Publish Time (in millis since epoch) that the changes were generated
        /// </summary>
        /// <value>Publish Time (in millis since epoch) that the changes were generated</value>        
        [JsonPropertyName("pt")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public long? PublishTime { get; set; }

        /// <summary>
        ///     Token value (non-null) should be stored and passed in a MarketSubscriptionMessage to resume subscription (in case of disconnect)
        /// </summary>
        /// <value>Token value (non-null) should be stored and passed in a MarketSubscriptionMessage to resume subscription (in case of disconnect)</value>        
        [JsonPropertyName("initialClk")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string InitialClk { get; set; } = null!;

        /// <summary>
        ///     Conflate Milliseconds - the conflation rate (may differ from that requested if subscription is delayed)
        /// </summary>
        /// <value>Conflate Milliseconds - the conflation rate (may differ from that requested if subscription is delayed)</value>        
        [JsonPropertyName("conflateMs")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public long? ConflateMs { get; set; }
    }
}

using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace BetfairStreamClient.ExchangeStream
{
    public class StatusMessage : RequestMessage
    {
        /// <summary>
        ///     The type of error in case of a failure
        /// </summary>
        /// <value>The type of error in case of a failure</value>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum ErrorCodeEnum
        {
            [JsonStringEnumMemberName("NO_APP_KEY")]
            NoAppKey,

            [JsonStringEnumMemberName("INVALID_APP_KEY")]
            InvalidAppKey,

            [JsonStringEnumMemberName("NO_SESSION")]
            NoSession,

            [JsonStringEnumMemberName("INVALID_SESSION_INFORMATION")]
            InvalidSessionInformation,

            [JsonStringEnumMemberName("NOT_AUTHORIZED")]
            NotAuthorized,

            [JsonStringEnumMemberName("INVALID_INPUT")]
            InvalidInput,

            [JsonStringEnumMemberName("INVALID_CLOCK")]
            InvalidClock,

            [JsonStringEnumMemberName("UNEXPECTED_ERROR")]
            UnexpectedError,

            [JsonStringEnumMemberName("TIMEOUT")]
            Timeout,

            [JsonStringEnumMemberName("SUBSCRIPTION_LIMIT_EXCEEDED")]
            SubscriptionLimitExceeded,

            [JsonStringEnumMemberName("INVALID_REQUEST")]
            InvalidRequest,

            [JsonStringEnumMemberName("CONNECTION_FAILED")]
            ConnectionFailed,

            [JsonStringEnumMemberName("MAX_CONNECTION_LIMIT_EXCEEDED")]
            MaxConnectionLimitExceeded
        }

        /// <summary>
        ///     The status of the last request
        /// </summary>
        /// <value>The status of the last request</value>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum StatusCodeEnum
        {
            [JsonStringEnumMemberName("SUCCESS")]
            Success,

            [JsonStringEnumMemberName("FAILURE")]
            Failure
        }

        /// <summary>
        ///     The type of error in case of a failure
        /// </summary>
        /// <value>The type of error in case of a failure</value>
        [JsonPropertyName("errorCode")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ErrorCodeEnum? ErrorCode { get; set; }

        /// <summary>
        ///     The status of the last request
        /// </summary>
        /// <value>The status of the last request</value>        
        [JsonPropertyName("statusCode")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public StatusCodeEnum? StatusCode { get; set; }

        [JsonPropertyName("errorMessage")]        
        public string ErrorMessage { get; set; } = null!;

        [JsonPropertyName("connectionId")]
        public string ConnectionId {  get; set; } = null!;

        [JsonPropertyName("connectionClosed")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? ConnectionClosed { get; set; }
    }
}

using System.Text.Json.Serialization;

namespace BFBot.Betting
{

    /// <summary>Base class for all errors raised by this client.</summary>
    public abstract class BetfairException : Exception
    {
        protected BetfairException(string message) : base(message) { }
        protected BetfairException(string message, Exception inner) : base(message, inner) { }
    }

    /// <summary>Raised when authentication with Betfair fails.</summary>
    public sealed class BetfairLoginException : BetfairException
    {
        public string Status { get; }
        public string? Error { get; }

        public BetfairLoginException(string status, string? error = null)
            : base($"Betfair login failed: status={status} error={error}")
        {
            Status = status;
            Error = error;
        }
    }

    /// <summary>Raised when a Betfair JSON-RPC call returns an HTTP-transport failure
    /// (non-2xx status, timeout, malformed body, etc.).</summary>
    public sealed class BetfairHttpException : BetfairException
    {
        public int StatusCode { get; }
        public string Body { get; }

        public BetfairHttpException(int statusCode, string body)
            : base($"Betfair HTTP error: status={statusCode} body={Truncate(body)}")
        {
            StatusCode = statusCode;
            Body = body;
        }

        private static string Truncate(string s) => s.Length > 500 ? s[..500] : s;
    }

    /// <summary>Raised when a Betfair JSON-RPC call succeeds at the HTTP layer but
    /// returns a JSON-RPC "error" object (an APINGException).</summary>
    public sealed class BetfairApiException : BetfairException
    {
        public string Method { get; }
        public string? ErrorCode { get; }
        public string? ErrorDetails { get; }
        public JsonRpcError RawError { get; }

        public BetfairApiException(string method, JsonRpcError rawError)
            : base(BuildMessage(method, rawError))
        {
            Method = method;
            RawError = rawError;
            ErrorCode = rawError.Data?.ApiNgException?.ErrorCode ?? rawError.Message;
            ErrorDetails = rawError.Data?.ApiNgException?.ErrorDetails;
        }

        private static string BuildMessage(string method, JsonRpcError rawError)
        {
            var code = rawError.Data?.ApiNgException?.ErrorCode ?? rawError.Message;
            var details = rawError.Data?.ApiNgException?.ErrorDetails;
            return $"Betfair API error calling '{method}': code={code} details={details}";
        }
    }

    // --- JSON-RPC error envelope shapes (used to populate BetfairApiException) ---

    public sealed class JsonRpcError
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("data")]
        public JsonRpcErrorData? Data { get; set; }
    }

    public sealed class JsonRpcErrorData
    {
        [JsonPropertyName("APINGException")]
        public ApiNgException? ApiNgException { get; set; }
    }

    public sealed class ApiNgException
    {
        [JsonPropertyName("errorCode")]
        public string? ErrorCode { get; set; }

        [JsonPropertyName("errorDetails")]
        public string? ErrorDetails { get; set; }
    }


}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BFBot.Betting
{
    internal sealed class JsonRpcRequest<TParams>
    {
        [JsonPropertyName("jsonrpc")]
        public string JsonRpc { get; set; } = "2.0";

        [JsonPropertyName("method")]
        public string Method { get; set; } = "";

        [JsonPropertyName("params")]
        public TParams? Params { get; set; }

        [JsonPropertyName("id")]
        public int Id { get; set; }
    }

}

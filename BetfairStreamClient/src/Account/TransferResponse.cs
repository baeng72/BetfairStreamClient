using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BetfairStreamClient.src.Account
{
    public class TransferResponse
    {
        [JsonPropertyName("transactionId")]
        public string TransactionId { get; set; }
    }
}

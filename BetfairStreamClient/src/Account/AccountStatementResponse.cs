using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BetfairStreamClient.src.Account
{
    public class AccountStatementReport
    {
        [JsonPropertyName("accountStatement")]
        public IList<StatementItem> AccountStatement { get; set; }

        [JsonPropertyName("moreAvailable")]
        public bool MoreAvailable { get; set; }
    }
}

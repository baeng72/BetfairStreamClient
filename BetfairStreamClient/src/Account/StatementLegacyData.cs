using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BetfairStreamClient.src.Account
{
    public class StatementLegacyData
    {
        [JsonPropertyName("avgPrice")]
        public double AveragePrice { get; set; }

        [JsonPropertyName("betSize")]
        public double BetSize { get; set; }

        [JsonPropertyName("betType")]
        public string BetType { get; set; }

        [JsonPropertyName("betCategoryType")]
        public string BetCategoryType { get; set; }

        [JsonPropertyName("commissionRate")]
        public string CommissionRate { get; set; }

        [JsonPropertyName("eventId")]
        public long EventId { get; set; }

        [JsonPropertyName("eventTypeId")]
        public long EventTypeId { get; set; }

        [JsonPropertyName("fullMarketName")]
        public string FullMarketName { get; set; }

        [JsonPropertyName("grossBetAmount")]
        public double GrossBetAmount { get; set; }

        [JsonPropertyName("marketName")]
        public string MarketName { get; set; }

        [JsonPropertyName("marketType")]
        public string MarketType { get; set; }

        [JsonPropertyName("placedDate")]
        public DateTime PlacedDate { get; set; }

        [JsonPropertyName("selectionId")]
        public long SelectionId { get; set; }

        [JsonPropertyName("selectionName")]
        public string SelectionName { get; set; }

        [JsonPropertyName("startDate")]
        public DateTime StartDate { get; set; }

        [JsonPropertyName("transactionType")]
        public string TransactionType { get; set; }

        [JsonPropertyName("transactionId")]
        public long TransactionId { get; set; }

        [JsonPropertyName("winLose")]
        public string WinLose { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BetfairStreamClient.src.Account
{
    public class AccountFundsResponse
    {
        [JsonPropertyName("availableToBetBalance")]
        public double AvailableToBetBalance { get; set; }

        [JsonPropertyName("exposure")]
        public double Exposure { get; set; }

        [JsonPropertyName("retainedCommission")]
        public double RetainedCommission { get; set; }

        [JsonPropertyName("exposureLimit")]
        public double ExposureLimit { get; set; }

        [JsonPropertyName("discountRate")]
        public double DiscountRate { get; set; }

        [JsonPropertyName("pointsBalance")]
        public double PointsBalance { get; set; }
    }
}

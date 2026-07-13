using System.Text.Json.Serialization;

namespace BFBot.Betting
{
    public sealed class LimitOrder
    {
        public LimitOrder(double size, double price, PersistenceType persistenceType = PersistenceType.LAPSE)
        {
            Size = size;
            Price = price;
            PersistenceType = persistenceType;
        }

        [JsonPropertyName("size")]
        public double Size { get; set; }

        [JsonPropertyName("price")]
        public double Price { get; set; }

        [JsonPropertyName("persistenceType")]
        public PersistenceType PersistenceType { get; set; }

        [JsonPropertyName("timeInForce")]
        public TimeInForce? TimeInForce { get; set; }

        [JsonPropertyName("minFillSize")]
        public double? MinFillSize { get; set; }
    }

}

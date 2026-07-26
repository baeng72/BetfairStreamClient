using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace BetfairStreamClient.ExchangeStream
{
    public enum SideEnum { Back, Lay }
    public enum PersistenceTypeEnum { LAPSE, PERSIST, MARKET_ON_CHANGE }

    public enum OrderTypeEnum { LIMIT, LIMIT_ON_CLOSE, MARKET_ON_CLOSE }

    public enum OrderStatusEnum { EXECUTABLE, EXECUTION_COMPLETE }

    public struct Order
    {
        public SideEnum Side;

        public PersistenceTypeEnum Persistence;

        public OrderTypeEnum OrderType;

        public OrderStatusEnum OrderStatus;

        public double SizeVoided;

        public double Price;

        public double SizeCancelled;

        //public string RegulatorCode;

        public double Size;

        public DateTime PlacedDate;                 //PlacedDate or DateTime.MinValue

        //public string RegulatorAuthorityCode;

        public DateTime MatchedDate;                //MatchedDate or DateTime.MinValue

        public double SizeLapsed;

        public double AveragePriceMatched;

        public double SizeMatched;

        public string BetId;

        //public double BSPLiability;

        public double SizeRemaining;

        public DateTime CancelledDate;                //CancelledDaet or DateTime.MinValue

        public string ReferenceOrder;

        public string ReferenceStrategy;




    }
}

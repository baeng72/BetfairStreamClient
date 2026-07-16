using BetfairStreamClient.Betting;

namespace StreamClientConsole{
    public abstract class OutboundCommand
    {
        public string MarketId { get; init; } = string.Empty;
    }

    // Your existing placement command updated to inherit the base
    public sealed class PlaceBetCommand : OutboundCommand
    {
        public string CustomerStrategyRef { get; init; } = string.Empty;    //E.g. ENTRY-0, TAKEPROF-1, STOPLOSS-2

        public string CustomerRef { get; init; } = string.Empty;   //E.g ST-MATCH-104 
        public long SelectionId { get; init; }
        public double Size { get; init; }
        public double Price { get; init; }
        public Side Side { get; init; } = Side.BACK; // BACK or LAY
        public PersistenceType PersistenceType { get; init; } = PersistenceType.PERSIST;


        public PlaceBetCommand(string marketId, long selectionId, Side side, double price, double size, PersistenceType persistenceType, string customerRef, string customerStrategyRef)
        {
            MarketId = marketId;
            SelectionId = selectionId;
            Size = size;
            Price = price;
            Side = side;
            PersistenceType = persistenceType;
            CustomerRef = customerRef;
            CustomerStrategyRef = customerStrategyRef;
        }
    }

    // Your new lightweight cancellation command
    public sealed class CancelBetCommand : OutboundCommand
    {
        public string BetId { get; init; } = string.Empty;

        public CancelBetCommand(string marketId, string betId)
        {
            MarketId = marketId;
            BetId = betId;
        }
    }
}

namespace BetfairStreamClient.Stream
{
public class OrderRunnerCache
    {
        private const int MaxActiveOrdersPerRunner = 100;
        private readonly OrderSnap[] _orders = new OrderSnap[MaxActiveOrdersPerRunner];
        public int ActiveCount { get; private set; } = 0;

        public void UpdateOrAddOrder(long betId, double price, double sizeRemaining, double sizeMatched, OrderSide side)
        {
            int matchIndex = -1;

            // Fast sequential span scan over localized primitives
            for (int i = 0; i < ActiveCount; i++)
            {
                if (_orders[i].BetId == betId)
                {
                    matchIndex = i;
                    break;
                }
            }

            // Determine status changes
            OrderStatus status = sizeRemaining == 0 ? OrderStatus.ExecutionComplete : (sizeMatched > 0 ? OrderStatus.ExecutionComplete : OrderStatus.Executable);

            if (matchIndex != -1)
            {
                if (status == OrderStatus.ExecutionComplete)
                {
                    // Order closed: swap the last active slot into this position to maintain array density
                    _orders[matchIndex] = _orders[ActiveCount - 1];
                    _orders[ActiveCount - 1] = default;
                    ActiveCount--;
                }
                else
                {
                    // Inline memory struct update
                    _orders[matchIndex] = new OrderSnap(betId, price, sizeRemaining, sizeMatched, side, status);
                }
            }
            else if (status != OrderStatus.ExecutionComplete && ActiveCount < MaxActiveOrdersPerRunner)
            {
                // Register brand new order
                _orders[ActiveCount++] = new OrderSnap(betId, price, sizeRemaining, sizeMatched, side, status);
            }
        }

        public int CopyActiveOrdersTo(OrderSnap[] destination, int startIndex)
        {
            for (int i = 0; i < ActiveCount; i++)
            {
                destination[startIndex++] = _orders[i];
            }
            return startIndex;
        }
    }
}
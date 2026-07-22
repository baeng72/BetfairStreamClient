
using System.Drawing;
using System.Runtime.Serialization;

namespace BetfairStreamClient.Stream
{
public class OrderRunnerCache
    {
        private const int MaxActiveOrdersPerRunner = 100;
        private const int MaxMatched = 20;
        private readonly OrderSnap[] _orders = new OrderSnap[MaxActiveOrdersPerRunner];
        private readonly PriceSize[] _matchedBacks = new PriceSize[MaxMatched];
        private readonly PriceSize[] _matchedLays = new PriceSize[MaxMatched];
        private int _matchedBackCount = 0;
        private int _matchedLayCount = 0;
        public int ActiveCount { get; private set; } = 0;

        public void ResetMatchedCount()
        {
            _matchedBackCount = _matchedLayCount = 0;
        }

        public void UpdateMatchedBack(double price, double size)
        {
            _matchedBacks[_matchedBackCount++] = new PriceSize(price, size);
            
        }
        public void UpdateMatchedLay(double price, double size)
        {
            _matchedLays[_matchedLayCount++] = new PriceSize(price, size);
        }

        public void UpdateOrAddOrder(long betId, double price, double sizeRemaining, double sizeMatched, double sizeVoid, SideEnum side, StatusEnum status, PtEnum persistence, OtEnum type)
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
            

            if (matchIndex != -1)
            {
                if (status == StatusEnum.EXECUTION_COMPLETE)
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
            else if (status != StatusEnum.EXECUTION_COMPLETE && ActiveCount < MaxActiveOrdersPerRunner)
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
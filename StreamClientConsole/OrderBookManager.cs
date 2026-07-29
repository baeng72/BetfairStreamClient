using System.Collections.Concurrent;
using BetfairStreamClient.ExchangeStream;
namespace StreamClientConsole
{
    public class OrderBookManager
    {
        // Key: betId, Value: Current execution status (e.g., "EXECUTABLE", "EXECUTION_COMPLETE")
        private readonly ConcurrentDictionary<long,Dictionary<string, OrderStatusEnum>> _activeOrders = new();

        // Call this when your Stream Handler receives an Order Change Message (OCM)
        public void UpdateOrderStatus(long selectionId, string betId, OrderStatusEnum status)
        {

            if (!_activeOrders.ContainsKey(selectionId))
                _activeOrders[selectionId] = new Dictionary<string, OrderStatusEnum>();            
           _activeOrders[selectionId][betId] = status;
        }

        public OrderStatusEnum GetOrderStatus(long selectionId, string betId)
        {
            return _activeOrders[selectionId].TryGetValue(betId, out var status) ? status : OrderStatusEnum.EXECUTION_COMPLETE;
        }

        public void RemoveOrder(long selectionId, string betId)
        {
            _activeOrders[selectionId].Remove(betId, out _);
        }
    }
}

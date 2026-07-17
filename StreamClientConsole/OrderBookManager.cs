using System.Collections.Concurrent;
using BetfairStreamClient.Stream;
namespace StreamClientConsole
{
    public class OrderBookManager
    {
        // Key: betId, Value: Current execution status (e.g., "EXECUTABLE", "EXECUTION_COMPLETE")
        private readonly ConcurrentDictionary<string, OrderStatus> _activeOrders = new();

        // Call this when your Stream Handler receives an Order Change Message (OCM)
        public void UpdateOrderStatus(string betId, OrderStatus status)
        {
            _activeOrders[betId] = status;
        }

        public OrderStatus GetOrderStatus(string betId)
        {
            return _activeOrders.TryGetValue(betId, out var status) ? status : OrderStatus.ExecutionComplete;
        }

        public void RemoveOrder(string betId)
        {
            _activeOrders.TryRemove(betId, out _);
        }
    }
}

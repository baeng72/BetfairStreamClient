using System.Collections.Concurrent;
using BetfairStreamClient.Stream;
namespace StreamClientConsole
{
    public class OrderBookManager
    {
        // Key: betId, Value: Current execution status (e.g., "EXECUTABLE", "EXECUTION_COMPLETE")
        private readonly ConcurrentDictionary<string, StatusEnum> _activeOrders = new();

        // Call this when your Stream Handler receives an Order Change Message (OCM)
        public void UpdateOrderStatus(string betId, StatusEnum status)
        {
            _activeOrders[betId] = status;
        }

        public StatusEnum GetOrderStatus(string betId)
        {
            return _activeOrders.TryGetValue(betId, out var status) ? status : StatusEnum.EXECUTION_COMPLETE;
        }

        public void RemoveOrder(string betId)
        {
            _activeOrders.TryRemove(betId, out _);
        }
    }
}

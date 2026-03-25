using DigitalProject.Request;
using DigitalProject.Response;

namespace DigitalProject.Interface.Orders
{
    public interface IOrderService
    {
        Task<OrderResponse> CreateOrderAsync(Guid userId, CreateOrderRequest request);
        Task<List<OrderResponse>> GetUserOrdersAsync(Guid userId);
        Task<OrderResponse?> GetOrderByIdAsync(Guid id);
    }
}

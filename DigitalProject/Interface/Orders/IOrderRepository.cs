using DigitalProject.Models;

namespace DigitalProject.Interface.Orders
{
    public interface IOrderRepository
    {
        Task<Order> CreateAsync(Order order);
        Task<List<Order>> GetByUserIdAsync(Guid userId);
        Task<Order?> GetByIdAsync(Guid id);
    }
}

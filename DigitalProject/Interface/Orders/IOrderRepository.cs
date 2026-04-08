using DigitalProject.Domain;
using DigitalProject.Models;
using DigitalProject.Request;
using DigitalProject.Response;

namespace DigitalProject.Interface.Orders
{
    public interface IOrderRepository
    {
        // 前台
        Task<Order> CreateAsync(Order order);
        Task<List<Order>> GetByUserIdAsync(Guid userId);
        Task<Order?> GetByIdAsync(Guid id);
        Task UpdateStatusAsync(Guid id, OrderStatus status);

        // 後台
        Task<List<Order>> GetAllAdminAsync(); 
        Task<Order?> GetByIdAdminAsync(Guid id);
    }
}
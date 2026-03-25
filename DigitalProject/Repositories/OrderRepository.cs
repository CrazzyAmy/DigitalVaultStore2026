using DigitalProject.Data;
using DigitalProject.Interface.Orders;
using DigitalProject.Models;
using Microsoft.EntityFrameworkCore;

namespace DigitalProject.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly DigitalVaultStoreDbContext _db;
        public OrderRepository(DigitalVaultStoreDbContext db)
        {
            _db = db;
        }
        public async Task<Order> CreateAsync(Order order)
        {
            _db.Orders.Add(order);
            await _db.SaveChangesAsync();
            return order;
        }

        public async Task<Order?> GetByIdAsync(Guid id)=>
        
            await _db.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id);
        

        public async Task<List<Order>> GetByUserIdAsync(Guid userId) =>
        
            await _db.Orders
             .Include(o => o.OrderItems)
             .Where(o => o.UserId == userId)
             .OrderByDescending(o => o.CreatedAt)
             .ToListAsync();

        
    }
}

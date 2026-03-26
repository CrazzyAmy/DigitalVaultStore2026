// Repositories/ReviewRepository.cs
using DigitalProject.Data;
using DigitalProject.Interface;
using DigitalProject.Interface.Reviews;
using DigitalProject.Models;
using Microsoft.EntityFrameworkCore;

namespace DigitalProject.Repositories.Reviews
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly DigitalVaultStoreDbContext _context;

        public ReviewRepository(DigitalVaultStoreDbContext context)
        {
            _context = context;
        }

        public async Task<List<Review>> GetByProductIdAsync(Guid productId)
        {
            return await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Product)
                .Where(r => r.ProductId == productId)
                .OrderByDescending(r => r.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<Review>> GetByUserIdAsync(Guid userId)
        {
            return await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Product)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Review?> GetByIdAsync(Guid id)
        {
            return await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Product)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        // 防止同一筆訂單對同一商品重複評論
        public async Task<bool> ExistsAsync(Guid userId, Guid productId, Guid orderId)
        {
            return await _context.Reviews
                .AnyAsync(r => r.UserId == userId
                            && r.ProductId == productId
                            && r.OrderId == orderId);
        }

        public async Task<bool> CreateAsync(Review review)
        {
            await _context.Reviews.AddAsync(review);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateAsync(Review review)
        {
            _context.Reviews.Update(review);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null) return false;

            _context.Reviews.Remove(review);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}

// Interface/IReviewRepository.cs
using DigitalProject.Models;

namespace DigitalProject.Interface.Reviews
{
    public interface IReviewRepository
    {
        Task<List<Review>> GetByProductIdAsync(Guid productId);
        Task<List<Review>> GetByUserIdAsync(Guid userId);
        Task<Review?> GetByIdAsync(Guid id);
        Task<bool> ExistsAsync(Guid userId, Guid productId, Guid orderId);
        Task<bool> CreateAsync(Review review);
        Task<bool> UpdateAsync(Review review);
        Task<bool> DeleteAsync(Guid id);
    }
}

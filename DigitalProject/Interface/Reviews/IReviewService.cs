// Interface/IReviewService.cs
using DigitalProject.Request;
using DigitalProject.Response;

namespace DigitalProject.Interface.Reviews
{
    public interface IReviewService
    {
        Task<List<ReviewResponse>> GetByProductIdAsync(Guid productId);
        Task<List<ReviewResponse>> GetByUserIdAsync(Guid userId);
        Task<ReviewResponse?> GetByIdAsync(Guid id);
        Task<(bool Success, string Message)> CreateAsync(Guid userId, CreateReviewRequest request);
        Task<(bool Success, string Message)> UpdateAsync(Guid userId, Guid reviewId, UpdateReviewRequest request);
        Task<(bool Success, string Message)> DeleteAsync(Guid userId, Guid reviewId);
    }
}
// Services/ReviewService.cs
using DigitalProject.Interface;
using DigitalProject.Interface.Reviews;
using DigitalProject.Models;
using DigitalProject.Request;
using DigitalProject.Response;

namespace DigitalProject.Services.Reviews
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository;

        public ReviewService(IReviewRepository reviewRepository)
        {
            _reviewRepository = reviewRepository;
        }

        public async Task<List<ReviewResponse>> GetByProductIdAsync(Guid productId)
        {
            var reviews = await _reviewRepository.GetByProductIdAsync(productId);
            return reviews.Select(MapToResponse).ToList();
        }

        public async Task<List<ReviewResponse>> GetByUserIdAsync(Guid userId)
        {
            var reviews = await _reviewRepository.GetByUserIdAsync(userId);
            return reviews.Select(MapToResponse).ToList();
        }

        public async Task<ReviewResponse?> GetByIdAsync(Guid id)
        {
            var review = await _reviewRepository.GetByIdAsync(id);
            return review == null ? null : MapToResponse(review);
        }

        public async Task<(bool Success, string Message)> CreateAsync(Guid userId, CreateReviewRequest request)
        {
            // 驗證評分範圍
            if (request.Rating < 1 || request.Rating > 5)
                return (false, "評分必須介於 1 到 5 之間");

            // 防止重複評論
            var exists = await _reviewRepository.ExistsAsync(userId, request.ProductId, request.OrderId);
            if (exists)
                return (false, "此訂單已對該商品評論過");

            var review = new Review
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ProductId = request.ProductId,
                OrderId = request.OrderId,
                Rating = request.Rating,
                Comment = request.Comment,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _reviewRepository.CreateAsync(review);
            return created ? (true, "評論新增成功") : (false, "評論新增失敗，請稍後再試");
        }

        public async Task<(bool Success, string Message)> UpdateAsync(Guid userId, Guid reviewId, UpdateReviewRequest request)
        {
            // 驗證評分範圍
            if (request.Rating < 1 || request.Rating > 5)
                return (false, "評分必須介於 1 到 5 之間");

            var review = await _reviewRepository.GetByIdAsync(reviewId);
            if (review == null)
                return (false, "評論不存在");

            // 只有本人可以修改
            if (review.UserId != userId)
                return (false, "無權限修改此評論");

            review.Rating = request.Rating;
            review.Comment = request.Comment;

            var updated = await _reviewRepository.UpdateAsync(review);
            return updated ? (true, "評論更新成功") : (false, "評論更新失敗，請稍後再試");
        }

        public async Task<(bool Success, string Message)> DeleteAsync(Guid userId, Guid reviewId)
        {
            var review = await _reviewRepository.GetByIdAsync(reviewId);
            if (review == null)
                return (false, "評論不存在");

            // 只有本人可以刪除
            if (review.UserId != userId)
                return (false, "無權限刪除此評論");

            var deleted = await _reviewRepository.DeleteAsync(reviewId);
            return deleted ? (true, "評論刪除成功") : (false, "評論刪除失敗，請稍後再試");
        }

        private static ReviewResponse MapToResponse(Review review) => new()
        {
            Id = review.Id,
            UserId = review.UserId,
            UserDisplayName = review.User?.DisplayName ?? string.Empty,
            ProductId = review.ProductId,
            ProductName = review.Product?.Name ?? string.Empty,
            OrderId = review.OrderId,
            Rating = review.Rating,
            Comment = review.Comment,
            CreatedAt = review.CreatedAt
        };
    }
}
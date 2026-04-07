using DigitalProject.Data;
using DigitalProject.Domain;
using DigitalProject.Exceptions;
using DigitalProject.Interface;
using DigitalProject.Interface.User;
using DigitalProject.Request;
using DigitalProject.Response;
using DigitalProject.Security;
using DigitalProject.Services.User;
using Microsoft.EntityFrameworkCore;

namespace DigitalProject.Services.User
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly DigitalVaultStoreDbContext _dbcontext;
        public UserService(IUserRepository userRepository, IPasswordHasher passwordHasher, DigitalVaultStoreDbContext dbcontext)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _dbcontext = dbcontext;
        }
        // 現在：同一個商品買多次會回傳多筆
        // 前端 key={p.productId} 就會重複

        public async Task<List<PurchaseResponse>> GetPurchasesAsync(Guid userId)
        {
            // 1. 先從資料庫撈資料
            var orderItems = await _dbcontext.OrderItems
                .Include(oi => oi.Product)
                    .ThenInclude(p => p.Category)
                .Include(oi => oi.Order)
                .Where(oi =>
                    oi.Order.UserId == userId &&
                    (oi.Order.Status == OrderStatus.Paid ||
                     oi.Order.Status == OrderStatus.Completed))
                .ToListAsync();  // ← 先 ToList，之後在記憶體做 GroupBy

            // 2. 在記憶體做 GroupBy 去重
            return orderItems
                .GroupBy(oi => oi.ProductId)
                .Select(g => g.OrderByDescending(oi => oi.Order.CreatedAt).First())
                .Select(oi => new PurchaseResponse
                {
                    ProductId = oi.ProductId,
                    Name = oi.Product.Name,
                    Price = oi.Product.Price,
                    ThumbnailUrl = string.IsNullOrEmpty(oi.Product.ThumbnailUrl)
                        ? $"https://picsum.photos/400/220?random={oi.ProductId}"
                        : oi.Product.ThumbnailUrl,
                    DownloadUrl = oi.Product.DownloadUrl,
                    CategoryName = oi.Product.Category.Name,
                    PurchasedAt = oi.Order.CreatedAt
                })
                .ToList();
        }

        public async Task UpdateDisplayNameAsync(Guid userId, UpdateDisplayNameRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.DisplayName))
                throw new AppException("顯示名稱不可為空", 400);

            var user = await _userRepository.GetByIdAsync(userId)
                ?? throw new AppException("找不到使用者", 404);

          
            await _userRepository.UpdateDisplayNameAsync(userId, request.DisplayName);
        }

        public async Task UpdatePasswordAsync(Guid userId, UpdatePasswordRequest request)
        {
            var user = await _userRepository.GetByIdAsync(userId)
                  ?? throw new AppException("找不到使用者", 404);

            // 驗證目前密碼
            if (!_passwordHasher.Verify(request.CurrentPassword, user.PasswordHash!))
                throw new AppException("目前密碼錯誤", 401);

            // 驗證新密碼長度
            if (request.NewPassword.Length < 8)
                throw new AppException("新密碼至少需要 8 個字元", 400);

            var newHash = _passwordHasher.Hash(request.NewPassword);
            await _userRepository.UpdatePasswordAsync(userId, newHash);
        }
    }
}

// Services/User/UserService.cs
using DigitalProject.Data;
using DigitalProject.Domain;
using DigitalProject.Exceptions;
using DigitalProject.Interface;
using DigitalProject.Interface.Role;
using DigitalProject.Interface.User;
using DigitalProject.Request;
using DigitalProject.Response;
using DigitalProject.Security;
using Microsoft.EntityFrameworkCore;

namespace DigitalProject.Services.User
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly DigitalVaultStoreDbContext _dbcontext;
        private readonly IRoleRepository _roleRepository;  

        public UserService(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            DigitalVaultStoreDbContext dbcontext,
            IRoleRepository roleRepository)  
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _dbcontext = dbcontext;
            _roleRepository = roleRepository;
        }

        // ── 前台 ──────────────────────────────────────────────

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

            if (!_passwordHasher.Verify(request.CurrentPassword, user.PasswordHash!))
                throw new AppException("目前密碼錯誤", 401);

            if (request.NewPassword.Length < 8)
                throw new AppException("新密碼至少需要 8 個字元", 400);

            var newHash = _passwordHasher.Hash(request.NewPassword);
            await _userRepository.UpdatePasswordAsync(userId, newHash);
        }

        public async Task<List<PurchaseResponse>> GetPurchasesAsync(Guid userId)
        {
            var orderItems = await _dbcontext.OrderItems
                .Include(oi => oi.Product)
                    .ThenInclude(p => p.Category)
                .Include(oi => oi.Order)
                .Where(oi =>
                    oi.Order.UserId == userId &&
                    (oi.Order.Status == OrderStatus.Paid ||
                     oi.Order.Status == OrderStatus.Completed))
                .ToListAsync();

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

        // ── 後台 ──────────────────────────────────────────────

        public async Task<IEnumerable<AdminUserResponse>> GetAllAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return users.Select(MapToAdminResponse);
        }

        public async Task<AdminUserResponse?> GetByIdAsync(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                throw new AppException("使用者不存在", 404);
            return MapToAdminResponse(user);
        }

        public async Task DeactivateAsync(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                throw new AppException("使用者不存在", 404);
            if (!user.IsActive)
                throw new AppException("此帳號已停用");

            await _userRepository.DeactivateAsync(id);
        }

        public async Task ActivateAsync(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                throw new AppException("使用者不存在", 404);
            if (user.IsActive)
                throw new AppException("此帳號已啟用");

            await _userRepository.ActivateAsync(id);
        }

        public async Task UpdateRoleAsync(Guid id, UpdateUserRoleRequest request)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                throw new AppException("使用者不存在", 404);

            var role = await _roleRepository.GetByCodeAsync(request.RoleCode);
            if (role == null)
                throw new AppException("角色不存在", 404);

            await _userRepository.UpdateRoleAsync(id, role.Id);
        }

        // ── MapToAdminResponse ─────────────────────────────────
        private static AdminUserResponse MapToAdminResponse(Models.User u) => new()
        {
            Id = u.Id,
            Email = u.Email,
            DisplayName = u.DisplayName,
            AvatarUrl = u.AvatarUrl,
            IsActive = u.IsActive,
            Provider = u.Provider.ToString(),
            CreatedAt = u.CreatedAt,
            Roles = u.UserRoles.Select(ur => ur.Role.Code).ToList()
        };
    }
}
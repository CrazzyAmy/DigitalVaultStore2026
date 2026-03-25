using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DigitalProject.Domain;
using DigitalProject.Request;
using DigitalProject.Response;
using DigitalProject.Interface;
using DigitalProject.Interface.User;
using DigitalProject.Models;
using DigitalProject.Security;
using Microsoft.IdentityModel.Tokens;
using DigitalProject.Interface.Auth;

namespace DigitalProject.Auth.Services
{
    public class AuthService(
        IUserRepository userRepo,
        IPasswordHasher passwordHasher,
        IConfiguration config) : IAuthService
    {
        public async Task<AuthResponse> RegisterAsync(RegisterRequest req)
        {
            if (await userRepo.ExistsByEmailAsync(req.Email))
                throw new InvalidOperationException("此 Email 已被註冊");

            DigitalProject.Models.User user = new User
            {
                Id = Guid.NewGuid(),
                Email = req.Email.ToLowerInvariant(),
                DisplayName = req.DisplayName,
                Role = UserRole.User,
                Provider = AuthProvider.Local,
                PasswordHash = passwordHasher.Hash(req.Password),  // Argon2id
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
            };

            await userRepo.AddAsync(user);
            await userRepo.SaveChangesAsync();

            return new AuthResponse(GenerateToken(user), ToResponse(user));
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest req)
        {
            var user = await userRepo.GetByEmailAsync(req.Email.ToLowerInvariant())
                ?? throw new UnauthorizedAccessException("帳號或密碼錯誤");

            if (!user.IsActive)
                throw new UnauthorizedAccessException("帳號已被停用");

            if (user.Provider != AuthProvider.Local || user.PasswordHash is null)
                throw new UnauthorizedAccessException("此帳號請使用 Google 登入");

            // Argon2id 驗證（constant-time compare，防 timing attack）
            if (!passwordHasher.Verify(req.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("帳號或密碼錯誤");

            return new AuthResponse(GenerateToken(user), ToResponse(user));
        }

        // ── Private ───────────────────────────────────────────────────────────

        private string GenerateToken(User user)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(config["Jwt:Secret"]!));

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub,   user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Role,               user.Role.ToString()),
                new Claim("displayName",                 user.DisplayName),
            };

            var token = new JwtSecurityToken(
                issuer: config["Jwt:Issuer"],
                audience: config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static UserResponse ToResponse(User u) =>
            new(u.Id, u.Email, u.DisplayName, u.AvatarUrl, u.Role);
    }
}

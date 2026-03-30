using DigitalProject.Domain;
using DigitalProject.Exceptions;
using DigitalProject.Interface;
using DigitalProject.Interface.Auth;
using DigitalProject.Interface.User;
using DigitalProject.Models;
using DigitalProject.Request;
using DigitalProject.Response;
using DigitalProject.Security;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DigitalProject.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtHelper _jwtHelper;
        public AuthService(IUserRepository userRepository, IPasswordHasher passwordHasher, IJwtHelper jwtHelper)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _jwtHelper = jwtHelper;
        } 
        public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
        {
            // 1. 檢查 Email 是否已存在
            if(await _userRepository.IsEmailExistsAsync(request.Email))
                throw new AppException("此 Email 已被註冊");
            // 2. 建立 User
            var user = new Models.User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                DisplayName = request.DisplayName,
                PasswordHash = _passwordHasher.Hash(request.Password),
                Provider = AuthProvider.Local,
                Role = UserRole.User,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
            };
            await _userRepository.CreateAsync(user);
            return new RegisterResponse
            {
                Message = "註冊成功，請使用 Email 登入",
                Email = user.Email,
                DisplayName = user.DisplayName,
            };
        }
        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            //1.查找User
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null)
                throw new AppException("Email或密碼錯誤");
            // 2. 驗證密碼
            if (!_passwordHasher.Verify(request.Password, user.PasswordHash!))
                throw new AppException("Email 或密碼錯誤", 401);
            // 3. 確認帳號啟用
            if (!user.IsActive)
                throw new AppException("此帳號已被停用", 401);
            // 4. 回傳 JWT
            return _jwtHelper.GenerateToken(user); 


        }

       
    }
}
// Controllers/AuthController.cs
using DigitalProject.Exceptions;
using DigitalProject.Interface.Auth;
using DigitalProject.Interface.Blacklist;
using DigitalProject.Request;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DigitalProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : BaseController
    {
        private readonly IAuthService _authService;
        private readonly ITokenBlacklistService _blacklistService;

        public AuthController(IAuthService authService, ITokenBlacklistService blacklistService)
        {
            _authService = authService;
            _blacklistService = blacklistService;
        }

        // POST /api/auth/register
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var result = await _authService.RegisterAsync(request);
            return Ok(result);
        }

        // POST /api/auth/login
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _authService.LoginAsync(request);
            return Ok(result);
        }

        // POST /api/auth/refresh
        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
                return BadRequest(new { error = "invalid_token" });

            try
            {
                var result = await _authService.RefreshAsync(request);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                // ex.Message 就是 "refresh_token_expired" 或 "refresh_token_revoked"
                return Unauthorized(new { error = ex.Message });
            }
        }

        // POST /api/auth/logout
        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout([FromBody] LogoutRequest request)
        {
            _blacklistService.Blacklist(
                request.Token,
                DateTime.UtcNow.AddDays(2)
            );
            return Ok(new { message = "登出成功" });
        }

        // GET /api/auth/google
        // 導向 Google 授權頁面
        [HttpGet("google")]
        [AllowAnonymous]
        public IActionResult GoogleLogin([FromQuery] string? returnUrl = "/")
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action("GoogleCallback", "Auth",
                    new { returnUrl }, Request.Scheme)
            };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        // GET /api/auth/google/callback
        // Google 授權後回呼
        [HttpGet("google/callback")]
        [AllowAnonymous]
        public async Task<IActionResult> GoogleCallback(string? returnUrl = "/")
        {
            // 1. 取得 Google 回傳的使用者資訊
            var result = await HttpContext.AuthenticateAsync(
                GoogleDefaults.AuthenticationScheme);

            if (!result.Succeeded)
                throw new AppException("Google 登入失敗", 401);

            var claims = result.Principal!.Claims;
            var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var name = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            var providerKey = claims.FirstOrDefault(c =>
                c.Type == ClaimTypes.NameIdentifier)?.Value;
            var avatarUrl = claims.FirstOrDefault(c =>
                c.Type == "urn:google:picture")?.Value;

            if (email == null || providerKey == null)
                throw new AppException("無法取得 Google 使用者資訊", 401);

            // 2. 登入或自動註冊
            var authResult = await _authService.GoogleLoginAsync(
                email, name ?? email, providerKey, avatarUrl);

            // 3. 導向前端並帶上 Token
            var frontendUrl = $"http://localhost:5173/auth/callback?token={authResult.Token}";
            return Redirect(frontendUrl);
        }
    }
}
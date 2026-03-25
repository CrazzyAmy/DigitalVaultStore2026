using System.Security.Claims;
using DigitalProject.Domain;
using DigitalProject.Request;
using DigitalProject.Response;
using DigitalProject.Interface;
using DigitalProject.Interface.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalProject.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        /// <summary>
        /// 註冊新帳號（Email / Password）
        /// </summary>
        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthResponse), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest req)
        {
            var result = await authService.RegisterAsync(req);
            return Ok(result);
        }

        /// <summary>
        /// 登入，回傳 JWT Token
        /// </summary>
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponse), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            var result = await authService.LoginAsync(req);
            return Ok(result);
        }

        /// <summary>
        /// 取得目前登入使用者資訊（需帶 JWT）
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(UserResponse), 200)]
        [ProducesResponseType(401)]
        public IActionResult Me()
        {
            var user = new UserResponse(
                Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!),
                User.FindFirstValue(ClaimTypes.Email)!,
                User.FindFirstValue("displayName")!,
                null,
                Enum.Parse<UserRole>(User.FindFirstValue(ClaimTypes.Role)!));

            return Ok(user);
        }
    }
}

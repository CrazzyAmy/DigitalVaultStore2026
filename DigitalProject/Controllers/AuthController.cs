// Controllers/AuthController.cs
using DigitalProject.Request;
using DigitalProject.Interface.Auth;
using Microsoft.AspNetCore.Mvc;

namespace DigitalProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // POST /api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var result = await _authService.RegisterAsync(request);
            return Ok(result);
        }

        // POST /api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _authService.LoginAsync(request);
            return Ok(result);
        }

        // POST /api/auth/refresh
        [HttpPost("refresh")]
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
    }
}
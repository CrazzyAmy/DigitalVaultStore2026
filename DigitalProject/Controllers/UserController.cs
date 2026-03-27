using DigitalProject.Interface.User;
using DigitalProject.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DigitalProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }
        // ← 加這個私有方法
        private Guid GetUserId() =>
            Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        [HttpPut("displayName")]
        public async Task<IActionResult> UpdateDisplayName([FromBody] UpdateDisplayNameRequest request)
        {
            await _userService.UpdateDisplayNameAsync(GetUserId(), request);
            return Ok(new { message = "顯示名稱更新成功" });
        }

        [HttpPut("password")]
        public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordRequest request)
        {
            await _userService.UpdatePasswordAsync(GetUserId(), request);
            return Ok(new { message = "密碼修改成功" });
        }

        [HttpGet("purchases")]
        public async Task<IActionResult> GetPurchases()
        {
            var purchases = await _userService.GetPurchasesAsync(GetUserId());
            return Ok(purchases);
        }
    }
}

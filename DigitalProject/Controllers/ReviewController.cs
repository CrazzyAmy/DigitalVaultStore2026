// Controllers/ReviewController.cs
using DigitalProject.Interface;
using DigitalProject.Interface.Reviews;
using DigitalProject.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace DigitalProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        // 取得商品的所有評論（公開）
        [HttpGet("product/{productId}")]
        public async Task<IActionResult> GetByProduct(Guid productId)
        {
            var reviews = await _reviewService.GetByProductIdAsync(productId);
            return Ok(reviews);
        }

        // 取得使用者自己的評論（需登入）
        [HttpGet("my")]
        [Authorize]
        public async Task<IActionResult> GetMyReviews()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var reviews = await _reviewService.GetByUserIdAsync(userId.Value);
            return Ok(reviews);
        }

        // 取得單筆評論
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var review = await _reviewService.GetByIdAsync(id);
            if (review == null) return NotFound(new { message = "評論不存在" });
            return Ok(review);
        }

        // 新增評論（需登入）
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateReviewRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var (success, message) = await _reviewService.CreateAsync(userId.Value, request);
            if (!success) return BadRequest(new { message });

            return Ok(new { message });
        }

        // 修改評論（需登入，只能改自己的）
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateReviewRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var (success, message) = await _reviewService.UpdateAsync(userId.Value, id, request);
            if (!success) return BadRequest(new { message });

            return Ok(new { message });
        }

        // 刪除評論（需登入，只能刪自己的）
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var (success, message) = await _reviewService.DeleteAsync(userId.Value, id);
            if (!success) return BadRequest(new { message });

            return Ok(new { message });
        }

        // 從 JWT Token 取出 UserId
        private Guid? GetUserId()
        {
            var userIdStr = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                         ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return Guid.TryParse(userIdStr, out var userId) ? userId : null;
        }
    }
}
using DigitalProject.Interface.Payment;
using DigitalProject.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DigitalProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : BaseController
    {
        private readonly IPaymentServie _paymentService;

        public PaymentController(IPaymentServie  paymentService)
        {
            _paymentService = paymentService;
        }

        // POST /api/payment
        // 信用卡或超商付款
        [HttpPost]
        public async Task<IActionResult> Pay([FromBody] PaymentRequest request)
        {
            var userId = GetUserId()!.Value;
            var result = await _paymentService.PayAsync(userId, request);
            return Ok(result);
        }

        // GET /api/payment/order/{orderId}
        // 取得訂單所有付款紀錄
        [HttpGet("order/{orderId}")]
        public async Task<IActionResult> GetByOrder(Guid orderId)
        {
            var payments = await _paymentService.GetByOrderIdAsync(orderId);
            return Ok(payments);
        }

        // PUT /api/payment/{id}/cvs-confirm
        // 模擬超商繳費完成
        [HttpPut("{id}/cvs-confirm")]
        public async Task<IActionResult> ConfirmCVS(Guid id)
        {
            var result = await _paymentService.ConfirmCVSPaymentAsync(id);
            return Ok(result);
        }

        // PUT /api/payment/{id}/void
        // 管理員作廢付款
        [HttpPut("{id}/void")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Void(Guid id, [FromBody] VoidPaymentRequest request)
        {
            var adminUserId = GetUserId()!.Value;
            var result = await _paymentService.VoidAsync(adminUserId, id, request.Reason);
            return Ok(result);
        }
    }
}

using DigitalProject.Interface.Orders;
using DigitalProject.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DigitalProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }
        // POST api/order
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOrderRequest request)
        {
            // Auth 完成後換成從 JWT 取得
            //var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var userId = Guid.Parse("1ED3C1A5-5D92-4D42-B29B-60957E3400A2");

            var order = await _orderService.CreateOrderAsync(userId, request);
            return Ok(order);
        }
            // GET api/order
            [HttpGet]
            public async Task<IActionResult> GetMyOrders()
            {
            var userId = Guid.Parse("1ED3C1A5-5D92-4D42-B29B-60957E3400A2");
            var orders = await _orderService.GetUserOrdersAsync(userId);
                return Ok(orders);
            }
        // GET api/order/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null) return NotFound();
            return Ok(order);
        }

    }
    }



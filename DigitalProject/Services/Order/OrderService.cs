using DigitalProject.Domain;
using DigitalProject.Exceptions;
using DigitalProject.Interface.Orders;
using DigitalProject.Interface.Prouduct;
using DigitalProject.Models;
using DigitalProject.Request;
using DigitalProject.Response;
namespace DigitalProject.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        public OrderService(IOrderRepository orderRepository,IProductRepository productRepository)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;

        }
        public async Task<OrderResponse> CreateOrderAsync(Guid userId, CreateOrderRequest request)
        {
            var products = (await _productRepository.GetByIdsAsync(request.ProductIds)).ToList();
            if (products.Count == 0)
                throw new AppException("找不到任何有效商品", 404);
            var items = products.Select(p=>new OrderItem
            {
                Id = Guid.NewGuid(),
                ProductId = p.Id,
                ProductName = p.Name,
                UnitPrice = p.Price,
                Quantity = 1,
                SubTotal = p.Price
            }).ToList();
            var order = new Order
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                OrderNo = "DV-" + Guid.NewGuid().ToString("N")[..8].ToUpper(),
                TotalAmount = items.Sum(i => i.SubTotal),
                Status = OrderStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                OrderItems = items,

            };
            await _orderRepository.CreateAsync(order);
            return MapToResponse(order);
           
        }
        public async Task<List<OrderResponse>> GetUserOrdersAsync(Guid userId)
        {
            var orders = await _orderRepository.GetByUserIdAsync(userId);
            return orders.Select(MapToResponse).ToList();

        }

        public async Task<OrderResponse?> GetOrderByIdAsync(Guid id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            return order == null ? null : MapToResponse(order);
        }

        private OrderResponse MapToResponse(Order o)
            => new()
            {
                Id = o.Id,
                OrderNo = o.OrderNo,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                CreatedAt = DateTime.UtcNow,
                Items = o.OrderItems.Select(i => new OrderItemResponse
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    UnitPrice = i.UnitPrice,
                    Quantity = i.Quantity,
                    SubTotal = i.SubTotal,
                }).ToList(),
            };
       

    }
}

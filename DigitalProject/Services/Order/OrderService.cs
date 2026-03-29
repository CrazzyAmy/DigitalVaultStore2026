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
        //取消訂單
        public async Task<bool> CancelOrderAsync(Guid userId, Guid orderId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
                throw new AppException("訂單不存在", 404);

            if (order.UserId != userId)
                throw new AppException("無權限取消此訂單", 403);

            if (order.Status != OrderStatus.Pending)
                throw new AppException("只有待付款的訂單可以取消");
            await _orderRepository.UpdateStatusAsync(orderId, OrderStatus.Cancelled);
            return true;

        }

        private OrderResponse MapToResponse(Order o)
     => new()
     {
         Id = o.Id,
         UserId = o.UserId,
         OrderNo = o.OrderNo,
         TotalAmount = o.TotalAmount,
         Status = o.Status,
         CreatedAt = o.CreatedAt,
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

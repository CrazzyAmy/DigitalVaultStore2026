using DigitalProject.Domain;

namespace DigitalProject.Response
{
    public class OrderResponse
    {
        public Guid Id { get; set; }
        public string OrderNo { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<OrderItemResponse> Items { get; set; } = new();
    }
}

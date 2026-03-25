namespace DigitalProject.Request
{
    public class CreateOrderRequest
    {
        public List<Guid> ProductIds { get; set; } = new();
    }
}

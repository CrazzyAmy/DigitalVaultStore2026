// Request/CreateReviewRequest.cs
namespace DigitalProject.Request
{
    public class CreateReviewRequest
    {
        public Guid ProductId { get; set; }
        public Guid OrderId { get; set; }
        public int Rating { get; set; }        // 1~5
        public string? Comment { get; set; }
    }
}
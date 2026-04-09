using DigitalProject.Request;
using DigitalProject.Response;

namespace DigitalProject.Interface.Payment
{
    public interface IPaymentServie
    {
        Task<PaymentResponse> PayAsync(Guid userId, PaymentRequest request);
        Task<List<PaymentResponse>> GetByOrderIdAsync(Guid orderId);
        Task<PaymentResponse> ConfirmCVSPaymentAsync(Guid paymentId);
        Task<PaymentResponse> VoidAsync(Guid adminUserId, Guid paymentId, string reason);
        Task<List<PaymentResponse>> GetAllAsync();
    }
}

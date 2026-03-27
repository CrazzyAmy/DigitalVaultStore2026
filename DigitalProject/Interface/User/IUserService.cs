using DigitalProject.Request;
using DigitalProject.Response;
using System.Threading.Tasks;

namespace DigitalProject.Interface.User
{
    public interface IUserService
    {
        Task UpdateDisplayNameAsync(Guid userId, UpdateDisplayNameRequest request);
        Task UpdatePasswordAsync(Guid userId, UpdatePasswordRequest request);
        Task<List<PurchaseResponse>> GetPurchasesAsync(Guid userId);

    }
}

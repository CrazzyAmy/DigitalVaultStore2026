using DigitalProject.Request;
using DigitalProject.Response;

namespace DigitalProject.Interface
{
    public interface IUserService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest req);
        Task<AuthResponse> LoginAsync(LoginRequest req);
    }
}

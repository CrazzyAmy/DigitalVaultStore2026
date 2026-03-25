using DigitalProject.Response;
using DigitalProject.Request;

namespace DigitalProject.Interface.Auth
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest req);
        Task<AuthResponse> LoginAsync(LoginRequest req);
    }
}

using DigitalProject.Response;
using DigitalProject.Request;

namespace DigitalProject.Interface.Auth
{
    public interface IAuthService
    {
        Task<RegisterResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> LoginAsync(LoginRequest request);

    }
}

using Microsoft.AspNetCore.Mvc;

namespace DigitalProject.Response
{
    public record AuthResponse(
        string Token,
        UserResponse User
    );
}

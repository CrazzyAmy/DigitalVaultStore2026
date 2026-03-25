using Microsoft.AspNetCore.Mvc;

namespace DigitalProject.Request
{
    public record RegisterRequest(
        string Email,
        string DisplayName,
        string Password
    );
}

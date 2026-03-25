using DigitalProject.Domain;
using Microsoft.AspNetCore.Mvc;

namespace DigitalProject.Response
{
    public record UserResponse(
        Guid Id,
        string Email,
        string DisplayName,
        string? AvatarUrl,
        UserRole Role
    );
}

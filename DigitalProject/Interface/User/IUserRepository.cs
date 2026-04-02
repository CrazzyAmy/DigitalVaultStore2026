using DigitalProject.Request;
using Microsoft.AspNetCore.Mvc;
using DigitalProject.Response;
using DigitalProject.Models;


namespace DigitalProject.Interface.User
{
    public interface IUserRepository
    {
        Task<Models.User?> GetByEmailAsync(string email);
        Task CreateAsync(Models.User user);
        Task<Models.User?> GetByIdAsync(Guid id);  
        Task<bool> IsEmailExistsAsync(string email);
        Task UpdateDisplayNameAsync(Guid id, string displayName);
        Task UpdatePasswordAsync(Guid id, string passwordHash);
        Task UpdateRefreshTokenAsync(Models.User user);
        Task<Models.User?> GetByRefreshTokenAsync(string refreshToken);
    }
}

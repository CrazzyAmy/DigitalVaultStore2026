using DigitalProject.Request;
using Microsoft.AspNetCore.Mvc;
using DigitalProject.Models;
using DigitalProject.Response;

namespace DigitalProject.Interface
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id);
        Task<User?> GetByEmailAsync(string email);
        Task<bool> IsEmailExistsAsync(string email);
        Task CreateAsync(User user);
        Task UpdateAsync(User user);
    }
}

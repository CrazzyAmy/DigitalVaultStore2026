using DigitalProject.Request;
using Microsoft.AspNetCore.Mvc;
using DigitalProject.Models;
using DigitalProject.Response;


namespace DigitalProject.Interface.Auth
{
    public interface IAuthRepository
    {
        Task<DigitalProject.Models.User?> GetByIdAsync(Guid id);
        Task<DigitalProject.Models.User?> GetByEmailAsync(string email);
        Task<bool> ExistsByEmailAsync(string email);
        Task AddAsync(DigitalProject.Models.User user);
        Task SaveChangesAsync();
    }
}

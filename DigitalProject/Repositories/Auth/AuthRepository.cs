using DigitalProject.Data;
using DigitalProject.Interface;
using DigitalProject.Models;
using Microsoft.EntityFrameworkCore;
using DigitalProject.Interface.Auth;

namespace DigitalProject.Repositories
{
    public class AuthRepository(DigitalVaultStoreDbContext db) : IAuthRepository
    {
        public Task<User?> GetByIdAsync(Guid id) =>
            db.Users.FindAsync(id).AsTask();

        public Task<User?> GetByEmailAsync(string email) =>
            db.Users.FirstOrDefaultAsync(u => u.Email == email);

        public Task<bool> ExistsByEmailAsync(string email) =>
            db.Users.AnyAsync(u => u.Email == email);

        public async Task AddAsync(User user) =>
            await db.Users.AddAsync(user);

        public Task SaveChangesAsync() =>
            db.SaveChangesAsync();
    }
}

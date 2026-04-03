using DigitalProject.Data;
using DigitalProject.Domain;
using DigitalProject.Interface.User;
using DigitalProject.Models;
using Microsoft.EntityFrameworkCore;

namespace DigitalProject.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly DigitalVaultStoreDbContext _dbcontext;

        public UserRepository(DigitalVaultStoreDbContext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        public async Task CreateAsync(User user)
        {
            await _dbcontext.Users.AddAsync(user);
            await _dbcontext.SaveChangesAsync();
        }

        public async Task<User?> GetByEmailAsync(string email)
         => await _dbcontext.Users
                .FirstOrDefaultAsync(u => u.Email == email);
        public async Task<User?> GetByIdAsync(Guid id)
            => await _dbcontext.Users.FindAsync(id);

        public async Task<bool> IsEmailExistsAsync(string email)
          => await _dbcontext.Users
              .AnyAsync(u => u.Email == email);

        public async Task UpdateDisplayNameAsync(Guid id, string displayName)
        {
            var user = await _dbcontext.Users.FindAsync(id);
            if (user == null) return;
            user.DisplayName = displayName;
            await _dbcontext.SaveChangesAsync();
        }

        public async Task UpdatePasswordAsync(Guid id, string passwordHash)
        {
            var user = await _dbcontext.Users.FindAsync(id);
            if (user == null) return;
            user.PasswordHash = passwordHash;
            await _dbcontext.SaveChangesAsync();
        }

        public async Task UpdateRefreshTokenAsync(User user)
        {
            _dbcontext.Users.Update(user);
            await _dbcontext.SaveChangesAsync();
        }

        public async Task<User?> GetByRefreshTokenAsync(string refreshToken)
        {
            return await _dbcontext.Users
                .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
        }

        // 用 Google ProviderKey 查找使用者
        public async Task<Models.User?> GetByProviderKeyAsync(string providerKey) =>
            await _dbcontext.Users
                .FirstOrDefaultAsync(u =>
                    u.Provider == AuthProvider.Google &&
                    u.ProviderKey == providerKey);
        public async Task UpdateAsync(User user)
        {
            _dbcontext.Users.Update(user);
            await _dbcontext.SaveChangesAsync();
        }
    }

}

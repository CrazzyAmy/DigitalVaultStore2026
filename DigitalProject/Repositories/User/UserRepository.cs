using DigitalProject.Data;
using DigitalProject.Interface;
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

        public async Task UpdateAsync(User user)
        {
            _dbcontext.Users.Update(user);
            await _dbcontext.SaveChangesAsync();
        }
    }

}

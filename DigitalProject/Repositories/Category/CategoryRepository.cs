using DigitalProject.Data;
using DigitalProject.Interface.Category;
using DigitalProject.Models;
using DigitalProject.Response;
using Microsoft.EntityFrameworkCore;

namespace DigitalProject.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly DigitalVaultStoreDbContext _context;
        public CategoryRepository(DigitalVaultStoreDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<CategoryResponse>> GetAllAsync()
        {
            return await _context.Categories
                .Where(c=>c.IsVisible)
                .OrderBy(c => c.SortOrder)
                .Select(c => new CategoryResponse
                {
                    Id = c.Id,
                    Name = c.Name,
                    Slug = c.Slug,
                    Description = c.Description,
                    SortOrder = c.SortOrder
                })
                .ToListAsync();
        }
    }
}

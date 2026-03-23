using DigitalProject.Data;
using DigitalProject.Interface.Prouduct;
using DigitalProject.Models;
using DigitalProject.Response;
using Microsoft.EntityFrameworkCore;

namespace DigitalProject.Repositories.Prouduct
{
    public class ProductRepository : IProductRepository
    {
        private readonly DigitalVaultStoreDbContext _context;
        public ProductRepository(DigitalVaultStoreDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<ProductResponse>> GetAllAsync(bool onlyPublish = true)
        {
           return await _context.Products
                .Where(p => !onlyPublish || p.IsPublished)
                .Include(p => p.Category)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p=>new ProductResponse 
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    ThumbnailUrl = string.IsNullOrEmpty(p.ThumbnailUrl)
        ? $"https://picsum.photos/400/220?random={p.Id}"
        : p.ThumbnailUrl,
                    DownloadUrl = p.DownloadUrl,
                    IsPublished = p.IsPublished,
                    CreatedAt = p.CreatedAt,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<ProductResponse>> GetByCategoryAsync(Guid categoryId)
        {
            return await _context.Products
            .Where(p => p.IsPublished && p.CategoryId == categoryId)
             .Include(p => p.Category)
             .OrderByDescending(p => p.CreatedAt)
             .Select(p => new ProductResponse
             {
                 Id = p.Id,
                 Name = p.Name,
                 Description = p.Description,
                 Price = p.Price,
                 ThumbnailUrl = string.IsNullOrEmpty(p.ThumbnailUrl)
        ? $"https://picsum.photos/400/220?random={p.Id}"
        : p.ThumbnailUrl,
                 DownloadUrl = p.DownloadUrl,
                 IsPublished = p.IsPublished,
                 CreatedAt = p.CreatedAt,
                 CategoryId = p.CategoryId,
                 CategoryName = p.Category.Name
             })
             .ToListAsync();
        }

        public async Task<ProductResponse?> GetByIdAsync(Guid id)
        {
            return await _context.Products
          .Where(p => p.Id == id && p.IsPublished)
          .Include(p => p.Category)
          .Select(p => new ProductResponse
          {
              Id = p.Id,
              Name = p.Name,
              Description = p.Description,
              Price = p.Price,
              ThumbnailUrl = string.IsNullOrEmpty(p.ThumbnailUrl)
        ? $"https://picsum.photos/400/220?random={p.Id}"
        : p.ThumbnailUrl,
              DownloadUrl = p.DownloadUrl,
              IsPublished = p.IsPublished,
              CreatedAt = p.CreatedAt,
              CategoryId = p.CategoryId,
              CategoryName = p.Category.Name
          })
          .FirstOrDefaultAsync();
        }

    }
}

using DigitalProject.Data;
using DigitalProject.Interface.Prouduct;
using DigitalProject.Models;
using DigitalProject.Request;
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

        // 後台查單一商品（不過濾 IsPublished）
        public async Task<ProductResponse?> GetByIdAdminAsync(Guid id)
        {
            return await _context.Products
                .Where(p => p.Id == id)//不加IsPublished過濾
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

        // 新增商品
        public async Task<Product> CreateAsync(CreateProductRequest request)
        {
            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                CategoryId = request.CategoryId,
                ThumbnailUrl = request.ThumbnailUrl,
                DownloadUrl = request.DownloadUrl,
                IsPublished = request.IsPublished,
                CreatedAt = DateTime.UtcNow,
            };
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
            return product;
        }

        // 編輯商品
        public async Task UpdateAsync(Guid id, UpdateProductRequest request)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return;

            product.Name = request.Name;
            product.Description = request.Description;
            product.Price = request.Price;
            product.CategoryId = request.CategoryId;
            product.ThumbnailUrl = request.ThumbnailUrl;
            product.DownloadUrl = request.DownloadUrl;
            product.IsPublished = request.IsPublished;

            await _context.SaveChangesAsync();
        }
        // 下架商品
        public  async Task UnpublishAsync(Guid id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return;
            product.IsPublished = false;
            await _context.SaveChangesAsync();
        }
        public async Task<IEnumerable<ProductResponse>> GetByIdsAsync(List<Guid> ids)
        {
            return await _context.Products
                 .Where(p => ids.Contains(p.Id) && p.IsPublished)
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
                   .ToListAsync();
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

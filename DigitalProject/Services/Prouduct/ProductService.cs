// Services/Product/ProductService.cs
using DigitalProject.Exceptions;
using DigitalProject.Interface.Prouduct;
using DigitalProject.Request;
using DigitalProject.Response;

namespace DigitalProject.Services.Prouduct
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;

        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        // ── 前台 ──────────────────────────────────────────────

        public async Task<IEnumerable<ProductResponse>> GetAllAsync(ProductQueryRequest query)
            => await _productRepository.GetAllAsync(query);

        public async Task<ProductResponse?> GetByIdAsync(Guid id)
            => await _productRepository.GetByIdAsync(id);

        // ── 後台 ──────────────────────────────────────────────

        public async Task<IEnumerable<ProductResponse>> GetAllAdminAsync()
        {
            // 後台查所有商品（含未發布）
            var query = new ProductQueryRequest();  // 空查詢條件
            var allProducts = await _productRepository.GetAllAsync(query);

            // 但 GetAllAsync 固定過濾 IsPublished
            // 所以後台要用 GetByIdAdminAsync 的概念
            // 直接用 Repository 的後台方法
            return await _productRepository.GetAllAdminAsync();
        }

        public async Task<ProductResponse?> GetByIdAdminAsync(Guid id)
        {
            var product = await _productRepository.GetByIdAdminAsync(id);
            if (product == null)
                throw new AppException("商品不存在", 404);
            return product;
        }

        public async Task<ProductResponse> CreateAsync(CreateProductRequest request)
        {
            var product = await _productRepository.CreateAsync(request);
            return new ProductResponse
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                ThumbnailUrl = product.ThumbnailUrl,
                DownloadUrl = product.DownloadUrl,
                IsPublished = product.IsPublished,
                CreatedAt = product.CreatedAt,
                CategoryId = product.CategoryId,
            };
        }

        public async Task UpdateAsync(Guid id, UpdateProductRequest request)
        {
            var product = await _productRepository.GetByIdAdminAsync(id);
            if (product == null)
                throw new AppException("商品不存在", 404);

            await _productRepository.UpdateAsync(id, request);
        }

        public async Task PublishAsync(Guid id)
        {
            var product = await _productRepository.GetByIdAdminAsync(id);
            if (product == null)
                throw new AppException("商品不存在", 404);
            if (product.IsPublished)
                throw new AppException("商品已上架");

            await _productRepository.PublishAsync(id);
        }

        public async Task UnpublishAsync(Guid id)
        {
            var product = await _productRepository.GetByIdAdminAsync(id);
            if (product == null)
                throw new AppException("商品不存在", 404);
            if (!product.IsPublished)
                throw new AppException("商品已下架");

            await _productRepository.UnpublishAsync(id);
        }
    }
}
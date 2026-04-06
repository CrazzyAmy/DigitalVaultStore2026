using DigitalProject.Exceptions;
using DigitalProject.Interface.Prouduct;
using DigitalProject.Models;
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
        public async Task<IEnumerable<ProductResponse>> GetAllAdminAsync()
        {
            return await _productRepository.GetAllAsync(onlyPublish: false);
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

        public async Task UnpublishAsync(Guid id)
        {
            var product = await _productRepository.GetByIdAdminAsync(id);
            if (product == null)
                throw new AppException("商品不存在", 404);

            await _productRepository.UnpublishAsync(id);
        }
        public async Task<IEnumerable<ProductResponse>> GetAllAsync(Guid? categoryId)
        {
            if (categoryId.HasValue)
            
            return await _productRepository.GetByCategoryAsync(categoryId.Value);

            return await _productRepository.GetAllAsync();
            
        }

        public async Task<ProductResponse?> GetByIdAsync(Guid id)
        {
            return await _productRepository.GetByIdAsync(id);
        }
    }
}

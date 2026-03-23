using DigitalProject.Interface.Prouduct;
using DigitalProject.Models;
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

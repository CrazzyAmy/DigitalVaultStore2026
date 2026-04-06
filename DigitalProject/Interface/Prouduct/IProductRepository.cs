using DigitalProject.Models;
using DigitalProject.Request;
using DigitalProject.Response;

namespace DigitalProject.Interface.Prouduct
{
    public interface IProductRepository
    {
        //前台
        Task<IEnumerable<ProductResponse>> GetAllAsync(bool onlyPublish = true);
        Task<IEnumerable<ProductResponse>> GetByCategoryAsync(Guid categoryId);
        Task<ProductResponse?> GetByIdAsync(Guid id);
        Task<IEnumerable<ProductResponse>> GetByIdsAsync(List<Guid> ids);
        // 後台
        Task<ProductResponse?> GetByIdAdminAsync(Guid id);    //不過濾 IsPublished
        Task<Product> CreateAsync(CreateProductRequest request);
        Task UpdateAsync(Guid id, UpdateProductRequest request);
        Task UnpublishAsync(Guid id);

    }
}

using DigitalProject.Models;
using DigitalProject.Request;
using DigitalProject.Response;

namespace DigitalProject.Interface.Prouduct
{
    public interface IProductService
    {
        //前台
        Task<IEnumerable<ProductResponse>> GetAllAsync(Guid? categoryId);
        Task<ProductResponse?>GetByIdAsync(Guid id);

        // 後台
        Task<IEnumerable<ProductResponse>> GetAllAdminAsync();
        Task<ProductResponse?> GetByIdAdminAsync(Guid id);
        Task<ProductResponse> CreateAsync(CreateProductRequest request);
        Task UpdateAsync(Guid id, UpdateProductRequest request);
        Task UnpublishAsync(Guid id);
    }
}

using DigitalProject.Models;
using DigitalProject.Response;

namespace DigitalProject.Interface.Prouduct
{
    public interface IProductRepository
    {
        Task<IEnumerable<ProductResponse>> GetAllAsync(bool onlyPublish = true);
        Task<IEnumerable<ProductResponse>> GetByCategoryAsync(Guid categoryId);
        Task<ProductResponse?> GetByIdAsync(Guid id);
        Task<IEnumerable<ProductResponse>> GetByIdsAsync(List<Guid> ids);

    }
}

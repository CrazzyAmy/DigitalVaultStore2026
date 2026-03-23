using DigitalProject.Models;
using DigitalProject.Response;

namespace DigitalProject.Interface.Prouduct
{
    public interface IProductService
    {
        Task<IEnumerable<ProductResponse>> GetAllAsync(Guid? categoryId);
        Task<ProductResponse?>GetByIdAsync(Guid id);
    }
}

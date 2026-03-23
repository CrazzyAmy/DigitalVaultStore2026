using DigitalProject.Models;
using DigitalProject.Response;

namespace DigitalProject.Interface
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryResponse>> GetAllAsync();
    }
}

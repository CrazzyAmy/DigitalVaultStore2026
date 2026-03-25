using DigitalProject.Models;
using DigitalProject.Response;

namespace DigitalProject.Interface.Category
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<CategoryResponse>> GetAllAsync();
    }
}

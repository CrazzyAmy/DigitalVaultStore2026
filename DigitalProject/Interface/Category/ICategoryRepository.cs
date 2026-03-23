
using DigitalProject.Models;
using DigitalProject.Response;

namespace DigitalProject.Interface.Categoy
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<CategoryResponse>> GetAllAsync();
    }
}

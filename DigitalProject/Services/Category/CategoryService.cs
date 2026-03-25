using DigitalProject.Interface;
using DigitalProject.Interface.Category;
using DigitalProject.Models;
using DigitalProject.Response;

namespace DigitalProject.Services
{
    public class CategoryService : ICategoryService     
    {
        private readonly ICategoryRepository _categoryRepository;
        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }
        public async Task<IEnumerable<CategoryResponse>> GetAllAsync()
        {
            return await  _categoryRepository.GetAllAsync();
        }
    }
}

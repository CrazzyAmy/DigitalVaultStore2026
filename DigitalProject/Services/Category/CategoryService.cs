using DigitalProject.Exceptions;
using DigitalProject.Interface;
using DigitalProject.Interface.Category;
using DigitalProject.Models;
using DigitalProject.Request;
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
        public async Task<IEnumerable<CategoryResponse>> GetAllAdminAsync() =>
        await _categoryRepository.GetAllAdminAsync();

        public async Task<CategoryResponse?> GetByIdAsync(Guid id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
                throw new AppException("分類不存在", 404);

            return new CategoryResponse
            {
                Id = category.Id,
                Name = category.Name,
                Slug = category.Slug,
                Description = category.Description,
                SortOrder = category.SortOrder,
                IsVisible = category.IsVisible
            };
        }
        public async Task<CategoryResponse> CreateAsync(CreateCategoryRequest request)
        {
            var category = await _categoryRepository.CreateAsync(request);
            return new CategoryResponse
            {
                Id = category.Id,
                Name = category.Name,
                Slug = category.Slug,
                Description = category.Description,
                SortOrder = category.SortOrder,
                IsVisible = category.IsVisible
            };
        }

        public async Task UpdateAsync(Guid id, UpdateCategoryRequest request)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
                throw new AppException("分類不存在", 404);

            await _categoryRepository.UpdateAsync(id, request);
        }

        public async Task DeleteAsync(Guid id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
                throw new AppException("分類不存在", 404);

            await _categoryRepository.DeleteAsync(id);
        }
    }
}

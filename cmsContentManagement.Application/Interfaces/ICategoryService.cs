using cmsContentManagement.Application.DTO;
using cmsContentManagement.Domain.Entities;

namespace cmsContentManagement.Application.Interfaces;

public interface ICategoryService
{
    Task<List<Category>> GetAllCategories();
    Task<Category?> GetCategoryById(Guid id);
    Task<Category> CreateCategory(CreateCategoryDTO categoryDto);
    Task UpdateCategory(CategoryDTO categoryDto);
    Task DeleteCategory(Guid id);
}

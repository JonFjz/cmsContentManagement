using cmsContentManagement.Application.DTO;
using cmsContentManagement.Domain.Entities;

namespace cmsContentManagement.Application.Interfaces;

public interface ICategoryService
{
    Task<List<CategoryResponseDTO>> GetAllCategories(Guid userId, int page, int pageSize, string? searchTerm = null);
    Task<Category?> GetCategoryById(Guid id);
    Task<Category> CreateCategory(Guid userId, CreateCategoryDTO categoryDto);
    Task UpdateCategory(CategoryDTO categoryDto);
    Task DeleteCategory(Guid id);
}

using cmsContentManagement.Application.DTO;
using cmsContentManagement.Application.Interfaces;
using cmsContentManagement.Domain.Entities;
using cmsContentManagment.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;

namespace cmsContentManagment.Infrastructure.Repositories;

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _dbContext;

    public CategoryService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Category>> GetAllCategories()
    {
        return await _dbContext.Categories.ToListAsync();
    }

    public async Task<Category?> GetCategoryById(Guid id)
    {
        return await _dbContext.Categories.FindAsync(id);
    }

    public async Task<Category> CreateCategory(CreateCategoryDTO categoryDto)
    {
        var category = new Category
        {
            CategoryId = Guid.NewGuid(),
            Name = categoryDto.Name,
            Description = categoryDto.Description
        };
        _dbContext.Categories.Add(category);
        await _dbContext.SaveChangesAsync();
        return category;
    }

    public async Task UpdateCategory(CategoryDTO categoryDto)
    {
        var category = await _dbContext.Categories.FindAsync(categoryDto.CategoryId);
        if (category != null)
        {
            category.Name = categoryDto.Name;
            category.Description = categoryDto.Description;
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task DeleteCategory(Guid id)
    {
        var category = await _dbContext.Categories.FindAsync(id);
        if (category != null)
        {
            _dbContext.Categories.Remove(category);
            await _dbContext.SaveChangesAsync();
        }
    }
}

using cmsContentManagement.Application.DTO;
using cmsContentManagement.Application.Interfaces;
using cmsContentManagement.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace cmsContentManagement.API.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<List<Category>> GetAllCategories()
    {
        return await _categoryService.GetAllCategories();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Category>> GetCategoryById(Guid id)
    {
        var category = await _categoryService.GetCategoryById(id);
        if (category == null) return NotFound();
        return category;
    }

    [HttpPost]
    public async Task<Category> CreateCategory(CreateCategoryDTO categoryDto)
    {
        return await _categoryService.CreateCategory(categoryDto);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateCategory(CategoryDTO categoryDto)
    {
        await _categoryService.UpdateCategory(categoryDto);
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(Guid id)
    {
        await _categoryService.DeleteCategory(id);
        return Ok();
    }
}

using cmsContentManagement.Application.DTO;
using cmsContentManagement.Application.Interfaces;
using cmsContentManagement.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using cmsContentManagement.API.Extensions;

namespace cmsContentManagement.API.Controllers;

[ApiController]
[Route("category")]
[Authorize]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<List<CategoryResponseDTO>> GetAllCategories([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
    {
        return await _categoryService.GetAllCategories(User.GetUserId(), page, pageSize, search);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Category>> GetCategoryById(Guid id)
    {
        var category = await _categoryService.GetCategoryById(id);
        if (category == null) return NotFound();
        return category;
    }

    [HttpPost]
    public async Task<ActionResult<Category>> CreateCategory(CreateCategoryDTO categoryDto)
    {
        try
        {
            return await _categoryService.CreateCategory(User.GetUserId(), categoryDto);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
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

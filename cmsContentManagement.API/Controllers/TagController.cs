using cmsContentManagement.Application.DTO;
using cmsContentManagement.Application.Interfaces;
using cmsContentManagement.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace cmsContentManagement.API.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class TagController : ControllerBase
{
    private readonly ITagService _tagService;

    public TagController(ITagService tagService)
    {
        _tagService = tagService;
    }

    [HttpGet]
    public async Task<List<Tag>> GetAllTags([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        return await _tagService.GetAllTags(page, pageSize);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Tag>> GetTagById(Guid id)
    {
        var tag = await _tagService.GetTagById(id);
        if (tag == null) return NotFound();
        return tag;
    }

    [HttpPost]
    public async Task<Tag> CreateTag(CreateTagDTO tagDto)
    {
        return await _tagService.CreateTag(tagDto);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateTag(TagDTO tagDto)
    {
        await _tagService.UpdateTag(tagDto);
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTag(Guid id)
    {
        await _tagService.DeleteTag(id);
        return Ok();
    }
}

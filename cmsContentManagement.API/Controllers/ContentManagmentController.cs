using System.Security.Claims;
using cmsContentManagement.API.Extensions;
using cmsContentManagement.Application.DTO;
using cmsContentManagement.Application.Interfaces;
using cmsContentManagement.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace cmsContentManagment.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class ContentManagmentController : ControllerBase
{
    private readonly IContentManagmentService _contentManagmentService;

    public ContentManagmentController(IContentManagmentService contentManagmentService)
    {
        _contentManagmentService = contentManagmentService;
    }

    [HttpGet("{contentId}")]
    public async Task<ContentDTO> GetContents(Guid contentId)
    {
        return await _contentManagmentService.getContentById(User.GetUserId(), contentId);
    }

    [HttpGet]
    public async Task<List<ContentDTO>> FilterContents(
        [FromQuery] string? query,
        [FromQuery] string? tag,
        [FromQuery] string? category,
        [FromQuery] string? status,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] bool withElastic = false
    )
    {
        return await _contentManagmentService.FilterContents(User.GetUserId(), query, tag, category, status, fromDate, toDate, page, pageSize, withElastic);
    }

    [HttpPost("{contentId}")]
    public async Task<IActionResult> SaveContent(Guid contentId, [FromBody] SaveContentDTO content)
    {
        await _contentManagmentService.CreateContent(User.GetUserId(), contentId, content);
        return Ok();
    }

    [HttpPut("{contentId}")]
    public async Task<IActionResult> UpdateContent(Guid contentId, [FromBody] SaveContentDTO content)
    {
        await _contentManagmentService.UpdateContent(User.GetUserId(), contentId, content);
        return Ok();
    }

    [HttpPost("{contentId}/unpublish")]
    public async Task<IActionResult> UnpublishContent(Guid contentId)
    {
        await _contentManagmentService.UnpublishContent(User.GetUserId(), contentId);
        return Ok();
    }

    [HttpDelete("{contentId}")]
    public async Task DeleteContent(Guid contentId)
    {
        await _contentManagmentService.DeleteContent(User.GetUserId(), contentId);
    }

    [HttpGet("generate-new-id")]
    public async Task<Guid> GenerateNewContentId()
    {
        return await _contentManagmentService.GenerateNewContentId(User.GetUserId());
    }
}

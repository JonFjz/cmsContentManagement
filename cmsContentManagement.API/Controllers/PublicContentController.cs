using cmsContentManagement.Application.Interfaces;
using cmsContentManagement.Application.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace cmsContentManagment.Controllers;

[ApiController]
[Route("api/public/content")]
public class PublicContentController : ControllerBase
{
    private readonly IContentManagmentService _contentManagmentService;

    public PublicContentController(IContentManagmentService contentManagmentService)
    {
        _contentManagmentService = contentManagmentService;
    }

    [HttpGet]
    public async Task<List<PublicContentDTO>> GetPublicContents(
        [FromQuery] string? query,
        [FromQuery] string? tag,
        [FromQuery] string? category,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        return await _contentManagmentService.GetPublicContents(query, tag, category, fromDate, toDate, page, pageSize);
    }
}

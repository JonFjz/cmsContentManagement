using System.Security.Claims;
using cmsContentManagement.Application.DTO;
using cmsContentManagement.Application.Interfaces;
using cmsContentManagement.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace cmsContentManagement.API.Controllers;

[ApiController]
[Route("api-key")]
[Authorize]
public class ApiKeyController : ControllerBase
{
    private readonly IApiKeyService _apiKeyService;

    public ApiKeyController(IApiKeyService apiKeyService)
    {
        _apiKeyService = apiKeyService;
    }

    [HttpPost("generate")]
    public async Task<ActionResult<ApiKeyResponse>> GenerateApiKey([FromBody] CreateApiKeyRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
        {
            // Fallback for demo if claim not present or name claim used
             var nameClaim = User.FindFirst(ClaimTypes.Name)?.Value;
             // In a real app we need the ID. Assuming for now we rely on proper JWT setup or finding the user.
             // If we can't find ID, unauthorized.
             if (Guid.TryParse(nameClaim, out Guid id)) userId = id;
             else return Unauthorized("User ID not found in token.");
        }

        var apiKey = await _apiKeyService.GenerateApiKeyAsync(userId, request.Description);

        return Ok(new ApiKeyResponse
        {
            Id = apiKey.Id,
            Key = apiKey.Key,
            Description = apiKey.Description,
            CreatedAt = apiKey.CreatedAt,
            IsActive = apiKey.IsActive
        });
    }

    [HttpDelete("{keyId}")]
    public async Task<IActionResult> RevokeApiKey(Guid keyId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
        {
             var nameClaim = User.FindFirst(ClaimTypes.Name)?.Value;
             if (Guid.TryParse(nameClaim, out Guid id)) userId = id;
             else return Unauthorized("User ID not found in token.");
        }

        var success = await _apiKeyService.RevokeApiKeyAsync(userId, keyId);
        if (!success)
        {
            return NotFound("API Key not found or does not belong to user.");
        }

        return NoContent();
    }

    [HttpGet]
    public async Task<ActionResult<List<ApiKeyResponse>>> GetMyApiKeys([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
        {
             var nameClaim = User.FindFirst(ClaimTypes.Name)?.Value;
             if (Guid.TryParse(nameClaim, out Guid id)) userId = id;
             else return Unauthorized("User ID not found in token.");
        }

        var keys = await _apiKeyService.GetUserApiKeysAsync(userId, page, pageSize);
        return Ok(keys.Select(k => new ApiKeyResponse
        {
            Id = k.Id,
            Key = k.Key,
            Description = k.Description,
            CreatedAt = k.CreatedAt,
            IsActive = k.IsActive
        }));
    }
}

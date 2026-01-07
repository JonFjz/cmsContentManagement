using cmsContentManagement.Domain.Entities;

namespace cmsContentManagement.Application.Interfaces;

public interface IApiKeyService
{
    Task<ApiKey> GenerateApiKeyAsync(Guid userId, string description);
    Task<bool> RevokeApiKeyAsync(Guid userId, Guid keyId);
    Task<ApiKey?> ValidateApiKeyAsync(string key);
    Task<List<ApiKey>> GetUserApiKeysAsync(Guid userId);
}

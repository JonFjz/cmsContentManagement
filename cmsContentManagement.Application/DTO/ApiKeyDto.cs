namespace cmsContentManagement.Application.DTO;

public class CreateApiKeyRequest
{
    public string Description { get; set; } = string.Empty;
}

public class ApiKeyResponse
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}

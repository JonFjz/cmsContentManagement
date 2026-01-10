using cmsContentManagement.Application.DTO;
using cmsContentManagement.Domain.Entities;

namespace cmsContentManagement.Application.Interfaces;

public interface IContentManagmentService
{
    public Task<Guid> GenerateNewContentId(Guid userId);
    public Task<ContentDTO> getContentById(Guid userId, Guid contentId);
    public Task<List<ContentDTO>> FilterContents(Guid userId, string? query, string? tag, string? category, string? status, DateTime? fromDate, DateTime? toDate, int page, int pageSize, bool withElastic = false);
    public Task DeleteContent(Guid userId, Guid contentId);
    public Task UnpublishContent(Guid userId, Guid contentId);
    public Task UpdateContent(Guid userId, Guid contentId, SaveContentDTO content);
    public Task AddAssetUrlToContent(Guid userId, Guid contentId, string assetUrl);
    public Task<List<PublicContentDTO>> GetPublicContents(string? query, string? tag, string? category, DateTime? fromDate, DateTime? toDate, int page, int pageSize, bool withElastic = false, string? apiKey = null);
    public Task<PublicContentDTO> GetPublicContentBySlug(string slug, string apiKey);
    public Task UpdateContentAssetUrl(Guid contentId, string assetUrl);
}

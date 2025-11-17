using cmsContentManagment.Application.DTO;
using cmsContentManagment.Domain.Entities;

namespace cmsContentManagment.Application.Interfaces;

public interface IContentManagmentService
{
    public Task<Guid> GenerateNewContentId(Guid userId); 
    public Task<Content> getContentById(Guid userId, Guid contentId);
    public Task<List<Content>> getAllContents(Guid userId);
    public Task DeleteContent(Guid userId, Guid contentId);
    public Task UpdateContent(ContentDTO content);
    public Task AddAssetUrlToContent(Guid userId, Guid contentId, string assetUrl);
}
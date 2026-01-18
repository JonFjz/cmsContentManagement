using cmsContentManagement.Application.DTO;
using cmsContentManagement.Domain.Entities;

namespace cmsContentManagement.Application.Interfaces;

public interface ITagService
{
    Task<List<TagDTO>> GetAllTags(Guid userId, int page, int pageSize, string? searchTerm = null);
    Task<Tag?> GetTagById(Guid id);
    Task<Tag> CreateTag(Guid userId, CreateTagDTO tagDto);
    Task UpdateTag(TagDTO tagDto);
    Task DeleteTag(Guid id);
}

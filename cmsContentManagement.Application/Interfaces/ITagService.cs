using cmsContentManagement.Application.DTO;
using cmsContentManagement.Domain.Entities;

namespace cmsContentManagement.Application.Interfaces;

public interface ITagService
{
    Task<List<Tag>> GetAllTags();
    Task<Tag?> GetTagById(Guid id);
    Task<Tag> CreateTag(CreateTagDTO tagDto);
    Task UpdateTag(TagDTO tagDto);
    Task DeleteTag(Guid id);
}

using cmsContentManagement.Application.DTO;
using cmsContentManagement.Application.Interfaces;
using cmsContentManagement.Domain.Entities;
using cmsContentManagment.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;

namespace cmsContentManagment.Infrastructure.Repositories;

public class TagService : ITagService
{
    private readonly AppDbContext _dbContext;

    public TagService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Tag>> GetAllTags()
    {
        return await _dbContext.Tags.ToListAsync();
    }

    public async Task<Tag?> GetTagById(Guid id)
    {
        return await _dbContext.Tags.FindAsync(id);
    }

    public async Task<Tag> CreateTag(CreateTagDTO tagDto)
    {
        var tag = new Tag
        {
            TagId = Guid.NewGuid(),
            Name = tagDto.Name
        };
        _dbContext.Tags.Add(tag);
        await _dbContext.SaveChangesAsync();
        return tag;
    }

    public async Task UpdateTag(TagDTO tagDto)
    {
        var tag = await _dbContext.Tags.FindAsync(tagDto.TagId);
        if (tag != null)
        {
            tag.Name = tagDto.Name;
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task DeleteTag(Guid id)
    {
        var tag = await _dbContext.Tags.FindAsync(id);
        if (tag != null)
        {
            _dbContext.Tags.Remove(tag);
            await _dbContext.SaveChangesAsync();
        }
    }
}

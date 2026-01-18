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

    public async Task<List<TagDTO>> GetAllTags(Guid userId, int page, int pageSize, string? searchTerm = null)
    {
        var query = _dbContext.Tags.Where(t => t.UserId == userId).AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(t => t.Name.Contains(searchTerm));
        }

        return await query
            .OrderBy(t => t.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TagDTO
            {
                TagId = t.TagId,
                Name = t.Name
            })
            .ToListAsync();
    }

    public async Task<Tag?> GetTagById(Guid id)
    {
        return await _dbContext.Tags.FindAsync(id);
    }

    public async Task<Tag> CreateTag(Guid userId, CreateTagDTO tagDto)
    {
        if (await _dbContext.Tags.AnyAsync(t => t.UserId == userId && t.Name == tagDto.Name))
        {
            throw new InvalidOperationException($"Tag with name '{tagDto.Name}' already exists.");
        }

        var tag = new Tag
        {
            TagId = Guid.NewGuid(),
            UserId = userId,
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

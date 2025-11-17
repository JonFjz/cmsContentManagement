using cmsContentManagment.Application.Common.ErrorCodes;
using cmsContentManagment.Application.DTO;
using cmsContentManagment.Application.Interfaces;
using cmsContentManagment.Domain.Entities;
using cmsContentManagment.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;

namespace cmsContentManagment.Infrastructure.Repositories;

public class ContentManagmentService : IContentManagmentService
{
    
    private readonly IDistributedCache _distributedCache;
    private readonly AppDbContext _dbContext;
    
    public async Task<Guid> GenerateNewContentId(Guid userId)
    {
        Content? content = await _dbContext.Contents.FirstOrDefaultAsync(e => e.UserId == userId && e.Status == "New");
        if (content != null) return content.ContentId;
        content = new Content()
        {
            UserId = userId
        };
        return content.ContentId;
    }

    public async Task<Content> getContentById(Guid userId, Guid contentId)
    {
        Content?  content = await _dbContext.Contents.FirstOrDefaultAsync(e => e.UserId == userId && e.ContentId == contentId);
        if(content == null) throw GeneralErrorCodes.NotFound;
        return content;
    }

    public async Task<List<Content>> getAllContents(Guid userId)
    {
        return _dbContext.Contents.Where(e => e.UserId == userId).ToList();
    }

    public async Task DeleteContent(Guid userId, Guid contentId)
    {
        Content? content = await _dbContext.Contents.FirstOrDefaultAsync(e => e.UserId == userId && e.ContentId ==  contentId);
        if(content != null) throw GeneralErrorCodes.NotFound;
        _dbContext.Contents.Remove(content);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateContent(ContentDTO content)
    {
        Content? contentToBeUpdated = await _dbContext.Contents.FirstOrDefaultAsync(e => e.UserId == content.UserId && e.ContentId ==  content.ContentId);
        if(contentToBeUpdated == null) throw GeneralErrorCodes.NotFound;
        contentToBeUpdated.Title = content.Title;
        contentToBeUpdated.RichContent = content.RichContent;
        await _dbContext.SaveChangesAsync();
        
    }
    
    public async Task AddAssetUrlToContent(Guid userId, Guid contentId, string assetUrl)
    {
        Content? content = await _dbContext.Contents.FirstOrDefaultAsync(e => e.UserId == userId && e.ContentId == contentId);
        if(content == null) throw GeneralErrorCodes.NotFound;
        content.AssetUrl = assetUrl;
        await _dbContext.SaveChangesAsync();
    }

}
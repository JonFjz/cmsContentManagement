using System;
using System.Collections.Generic;
using System.Linq;
using cmsContentManagement.Application.Common.ErrorCodes;
using cmsContentManagement.Application.Common.Settings;
using cmsContentManagement.Application.DTO;
using cmsContentManagement.Application.Interfaces;
using cmsContentManagement.Domain.Entities;
using cmsContentManagment.Infrastructure.Persistance;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Search;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace cmsContentManagment.Infrastructure.Repositories;

public class ContentManagmentService : IContentManagmentService
{
    private readonly AppDbContext _dbContext;
    private readonly ElasticsearchClient _elasticClient;
    private readonly ElasticSettings _elasticSettings;
    private readonly ILogger<ContentManagmentService> _logger;

    public ContentManagmentService(
        AppDbContext dbContext,
        ElasticsearchClient elasticClient,
        IOptions<ElasticSettings> elasticOptions,
        ILogger<ContentManagmentService> logger)
    {
        _dbContext = dbContext;
        _elasticClient = elasticClient;
        _logger = logger;
        _elasticSettings = elasticOptions.Value;
    }

    public async Task<Guid> GenerateNewContentId(Guid userId)
    {
        var content = await _dbContext.Contents.FirstOrDefaultAsync(e => e.UserId == userId && e.Status == "New");
        if (content != null) return content.ContentId;

        content = new Content
        {
            UserId = userId
        };

        await _dbContext.Contents.AddAsync(content);
        await _dbContext.SaveChangesAsync();
        await IndexContentAsync(content);

        return content.ContentId;
    }

    public async Task<ContentDTO> getContentById(Guid userId, Guid contentId)
    {
        var content = await _dbContext.Contents
            .Include(c => c.Category)
            .Include(c => c.Tags)
            .FirstOrDefaultAsync(e => e.UserId == userId && e.ContentId == contentId);
        
        if (content == null) throw GeneralErrorCodes.NotFound;

        return new ContentDTO
        {
            ContentId = content.ContentId,
            AssetUrl = content.AssetUrl,
            Status = content.Status,
            Title = content.Title,
            RichContent = content.RichContent,
            UserId = content.UserId,
            CategoryId = content.CategoryId,
            CategoryName = content.Category?.Name,
            CreatedOn = content.CreatedOn,
            UpdatedOn = content.UpdatedOn,
            Tags = content.Tags.Select(t => new TagDTO { TagId = t.TagId, Name = t.Name }).ToList()
        };
    }

    public async Task<List<ContentDTO>> FilterContents(Guid userId, string? query, string? tag, string? category, string? status, DateTime? fromDate, DateTime? toDate, int page, int pageSize)
    {
        var queryable = _dbContext.Contents
            .Include(c => c.Category)
            .Include(c => c.Tags)
            .Where(e => e.UserId == userId && e.Status != "Deleted");

        if (!string.IsNullOrWhiteSpace(query))
        {
            queryable = queryable.Where(c => c.Title.Contains(query) || (c.RichContent != null && c.RichContent.Contains(query)));
        }

        if (!string.IsNullOrWhiteSpace(tag))
        {
            queryable = queryable.Where(c => c.Tags.Any(t => t.Name == tag));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            queryable = queryable.Where(c => c.Category != null && c.Category.Name == category);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            queryable = queryable.Where(c => c.Status == status);
        }

        if (fromDate.HasValue)
        {
            queryable = queryable.Where(c => c.CreatedOn >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            queryable = queryable.Where(c => c.CreatedOn <= toDate.Value);
        }

        var contents = await queryable
            .OrderByDescending(c => c.CreatedOn)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return contents.Select(c => new ContentDTO
        {
            ContentId = c.ContentId,
            AssetUrl = c.AssetUrl,
            Status = c.Status,
            Title = c.Title,
            RichContent = c.RichContent,
            UserId = c.UserId,
            CategoryId = c.CategoryId,
            CategoryName = c.Category?.Name,
            CreatedOn = c.CreatedOn,
            UpdatedOn = c.UpdatedOn,
            Tags = c.Tags.Select(t => new TagDTO { TagId = t.TagId, Name = t.Name }).ToList()
        }).ToList();
    }

    public async Task DeleteContent(Guid userId, Guid contentId)
    {
        var content = await _dbContext.Contents.FirstOrDefaultAsync(e => e.UserId == userId && e.ContentId == contentId);
        if (content == null) throw GeneralErrorCodes.NotFound;

        content.Status = "Deleted";
        await _dbContext.SaveChangesAsync();
        await RemoveContentFromIndexAsync(contentId);
    }

    public async Task CreateContent(Guid userId, Guid contentId, SaveContentDTO content)
    {
        var contentToBeUpdated = await _dbContext.Contents
            .Include(c => c.Tags)
            .Include(c => c.Category)
            .FirstOrDefaultAsync(e => e.UserId == userId && e.ContentId == contentId);
        
        if (contentToBeUpdated == null) throw GeneralErrorCodes.NotFound;

        if (contentToBeUpdated.Status != "New")
        {
            throw GeneralErrorCodes.ContentAlreadyExists;
        }

        await DoSaveContent(contentToBeUpdated, content);
    }

    public async Task UpdateContent(Guid userId, Guid contentId, SaveContentDTO content)
    {
        var contentToBeUpdated = await _dbContext.Contents
            .Include(c => c.Tags)
            .Include(c => c.Category)
            .FirstOrDefaultAsync(e => e.UserId == userId && e.ContentId == contentId);
        
        if (contentToBeUpdated == null) throw GeneralErrorCodes.NotFound;

        if (contentToBeUpdated.Status == "New")
        {
            throw GeneralErrorCodes.ContentIsNew;
        }

        await DoSaveContent(contentToBeUpdated, content);
    }

    private async Task DoSaveContent(Content contentToBeUpdated, SaveContentDTO content)
    {
        contentToBeUpdated.Title = content.Title;
        contentToBeUpdated.RichContent = content.RichContent;
        
        // Handle Category by Name
        if (!string.IsNullOrWhiteSpace(content.CategoryName))
        {
            var category = await _dbContext.Categories.FirstOrDefaultAsync(c => c.Name == content.CategoryName);
            if (category == null)
            {
                category = new Category
                {
                    CategoryId = Guid.NewGuid(),
                    Name = content.CategoryName,
                    Description = "Created automatically"
                };
                _dbContext.Categories.Add(category);
            }
            contentToBeUpdated.Category = category;
        }
        else
        {
            contentToBeUpdated.CategoryId = null;
        }

        if (!string.IsNullOrWhiteSpace(contentToBeUpdated.Title) && !string.IsNullOrWhiteSpace(contentToBeUpdated.RichContent))
        {
            contentToBeUpdated.Status = "Published";
        }
        else
        {
            contentToBeUpdated.Status = "Draft";
        }

        if (!string.IsNullOrEmpty(content.AssetUrl))
        {
            contentToBeUpdated.AssetUrl = content.AssetUrl;
        }

        contentToBeUpdated.UpdatedOn = DateTime.UtcNow;

        // Update Tags by Name
        contentToBeUpdated.Tags.Clear();
        if (content.Tags != null && content.Tags.Any())
        {
            foreach (var tagName in content.Tags)
            {
                var tag = await _dbContext.Tags.FirstOrDefaultAsync(t => t.Name == tagName);
                if (tag == null)
                {
                    throw new Exception($"Tag '{tagName}' does not exist.");
                }
                contentToBeUpdated.Tags.Add(tag);
            }
        }

        await _dbContext.SaveChangesAsync();
        await IndexContentAsync(contentToBeUpdated);
    }

    public async Task UnpublishContent(Guid userId, Guid contentId)
    {
        var content = await _dbContext.Contents.FirstOrDefaultAsync(e => e.UserId == userId && e.ContentId == contentId);
        if (content == null) throw GeneralErrorCodes.NotFound;

        content.Status = "Unpublished";
        await _dbContext.SaveChangesAsync();
        await IndexContentAsync(content);
    }

    public async Task AddAssetUrlToContent(Guid userId, Guid contentId, string assetUrl)
    {
        var content = await _dbContext.Contents.FirstOrDefaultAsync(e => e.UserId == userId && e.ContentId == contentId);
        if (content == null) throw GeneralErrorCodes.NotFound;

        content.AssetUrl = assetUrl;

        await _dbContext.SaveChangesAsync();
        await IndexContentAsync(content);
    }

    public async Task<List<PublicContentDTO>> GetPublicContents(string? query, string? tag, string? category, DateTime? fromDate, DateTime? toDate, int page, int pageSize)
    {
        var queryable = _dbContext.Contents
            .Include(c => c.Category)
            .Include(c => c.Tags)
            .Where(c => c.Status == "Published");

        if (!string.IsNullOrWhiteSpace(query))
        {
            queryable = queryable.Where(c => c.Title.Contains(query) || c.RichContent.Contains(query));
        }

        if (!string.IsNullOrWhiteSpace(tag))
        {
            queryable = queryable.Where(c => c.Tags.Any(t => t.Name == tag));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            queryable = queryable.Where(c => c.Category != null && c.Category.Name == category);
        }

        if (fromDate.HasValue)
        {
            queryable = queryable.Where(c => c.CreatedOn >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            queryable = queryable.Where(c => c.CreatedOn <= toDate.Value);
        }

        return await queryable
            .OrderByDescending(c => c.CreatedOn)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new PublicContentDTO
            {
                ContentId = c.ContentId,
                Title = c.Title,
                RichContent = c.RichContent,
                AssetUrl = c.AssetUrl,
                Status = c.Status,
                CreatedOn = c.CreatedOn,
                UpdatedOn = c.UpdatedOn,
                UserId = c.UserId,
                Category = c.Category == null ? null : new CategoryDTO
                {
                    CategoryId = c.Category.CategoryId,
                    Name = c.Category.Name,
                    Description = c.Category.Description
                },
                Tags = c.Tags.Select(t => new TagDTO
                {
                    TagId = t.TagId,
                    Name = t.Name
                }).ToList()
            })
            .ToListAsync();
    }

    private async Task IndexContentAsync(Content content)
    {
        try
        {
            var response = await _elasticClient.IndexAsync(content, i => i
                .Index(_elasticSettings.DefaultIndex)
                .Id(content.ContentId.ToString()));

            if (!response.IsValidResponse)
            {
                _logger.LogWarning("Failed to index content {ContentId}: {Reason}", content.ContentId, response.DebugInformation);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while indexing content {ContentId}", content.ContentId);
        }
    }

    private async Task RemoveContentFromIndexAsync(Guid contentId)
    {
        try
        {
            var response = await _elasticClient.DeleteAsync<Content>(contentId.ToString(), d => d.Index(_elasticSettings.DefaultIndex));

            if (!response.IsValidResponse)
            {
                _logger.LogWarning("Failed to remove content {ContentId} from index: {Reason}", contentId, response.DebugInformation);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting content {ContentId} from index", contentId);
        }
    }
}
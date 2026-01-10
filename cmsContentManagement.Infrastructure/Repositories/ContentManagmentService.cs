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
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace cmsContentManagment.Infrastructure.Repositories;

public class ContentManagmentService : IContentManagmentService
{
    private readonly AppDbContext _dbContext;
    private readonly ElasticsearchClient _elasticClient;
    private readonly ElasticSettings _elasticSettings;
    private readonly ILogger<ContentManagmentService> _logger;
    private readonly IApiKeyService _apiKeyService;
    private readonly IDistributedCache _cache;

    public ContentManagmentService(
        AppDbContext dbContext,
        ElasticsearchClient elasticClient,
        IOptions<ElasticSettings> elasticOptions,
        ILogger<ContentManagmentService> logger,
        IApiKeyService apiKeyService,
        IDistributedCache cache)
    {
        _dbContext = dbContext;
        _elasticClient = elasticClient;
        _logger = logger;
        _elasticSettings = elasticOptions.Value;
        _apiKeyService = apiKeyService;
        _cache = cache;
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

    public async Task<List<ContentDTO>> FilterContents(Guid userId, string? query, string? tag, string? category, string? status, DateTime? fromDate, DateTime? toDate, int page, int pageSize, bool withElastic = false)
    {
        if (withElastic)
        {
            var response = await _elasticClient.SearchAsync<Content>(s => s
                .Indices(_elasticSettings.DefaultIndex)
                .From((page - 1) * pageSize)
                .Size(pageSize)
                .Sort(sort => sort.Field(f => f.CreatedOn, d => d.Order(SortOrder.Desc)))
                .Query(q => q
                    .Bool(b =>
                    {
                        var must = new List<Action<QueryDescriptor<Content>>>();

                        must.Add(m => m.Term(t => t.Field("userId.keyword").Value(userId.ToString())));
                        b.MustNot(mn => mn.Term(t => t.Field(f => f.Status.Suffix("keyword")).Value("Deleted")));

                        if (!string.IsNullOrWhiteSpace(query))
                        {
                            must.Add(m => m.MultiMatch(mm => mm
                                .Fields(new [] { "title", "richContent" })
                                .Query(query)
                                .Fuzziness(new Fuzziness("AUTO"))
                            ));
                        }

                        if (!string.IsNullOrWhiteSpace(tag))
                        {
                            must.Add(m => m.Term(t => t.Field("tags.name.keyword").Value(tag)));
                        }

                        if (!string.IsNullOrWhiteSpace(category))
                        {
                            must.Add(m => m.Term(t => t.Field("category.name.keyword").Value(category)));
                        }

                        if (!string.IsNullOrWhiteSpace(status))
                        {
                            must.Add(m => m.Term(t => t.Field(f => f.Status.Suffix("keyword")).Value(status)));
                        }

                        if (fromDate.HasValue)
                        {
                            must.Add(m => m.Range(r => r.Date(dr => dr.Field(f => f.CreatedOn).Gte(fromDate.Value))));
                        }

                        if (toDate.HasValue)
                        {
                            must.Add(m => m.Range(r => r.Date(dr => dr.Field(f => f.CreatedOn).Lte(toDate.Value))));
                        }

                        b.Must(must.ToArray());
                    })
                )
            );

            if (response.IsValidResponse)
            {
                return response.Documents.Select(c => new ContentDTO
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
                    Tags = c.Tags?.Select(t => new TagDTO { TagId = t.TagId, Name = t.Name }).ToList() ?? new List<TagDTO>()
                }).ToList();
            }
            
            _logger.LogError("Elastic search failed: {Reason}", response.DebugInformation);
        }

        var queryable = _dbContext.Contents
            .Include(c => c.Category)
            .Include(c => c.Tags)
            .Where(e => e.UserId == userId && e.Status != "Deleted");

        if (!string.IsNullOrWhiteSpace(query))
        {
            queryable = queryable.Where(c => (c.Title != null && c.Title.Contains(query)) || (c.RichContent != null && c.RichContent.Contains(query)));
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
        if (!string.IsNullOrEmpty(content.Slug))
        {
            await _cache.RemoveAsync(content.Slug);
        }
    }

    public async Task UpdateContent(Guid userId, Guid contentId, SaveContentDTO content)
    {
        var contentToBeUpdated = await _dbContext.Contents
            .Include(c => c.Tags)
            .Include(c => c.Category)
            .FirstOrDefaultAsync(e => e.UserId == userId && e.ContentId == contentId);
        
        if (contentToBeUpdated == null) throw GeneralErrorCodes.NotFound;

        await DoSaveContent(contentToBeUpdated, content);
    }

    private async Task DoSaveContent(Content contentToBeUpdated, SaveContentDTO content)
    {
        contentToBeUpdated.Title = content.Title;
        if (string.IsNullOrEmpty(contentToBeUpdated.Slug) && !string.IsNullOrWhiteSpace(content.Title))
        {
             var slugTitle = content.Title.Trim().ToLowerInvariant().Replace(" ", "-");
             contentToBeUpdated.Slug = $"/{slugTitle}";
        }

        contentToBeUpdated.RichContent = content.RichContent;

        contentToBeUpdated.AssetUrl = content.AssetUrl;
        
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

        contentToBeUpdated.UpdatedOn = DateTime.UtcNow;

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
        if (!string.IsNullOrEmpty(contentToBeUpdated.Slug))
        {
            await _cache.RemoveAsync(contentToBeUpdated.Slug);
        }
    }

    public async Task UnpublishContent(Guid userId, Guid contentId)
    {
        var content = await _dbContext.Contents
            .Include(c => c.Category)
            .Include(c => c.Tags)
            .FirstOrDefaultAsync(e => e.UserId == userId && e.ContentId == contentId);
        if (content == null) throw GeneralErrorCodes.NotFound;

        content.Status = "Unpublished";
        await _dbContext.SaveChangesAsync();
        await IndexContentAsync(content);
        if (!string.IsNullOrEmpty(content.Slug))
        {
            await _cache.RemoveAsync(content.Slug);
        }
    }

    public async Task AddAssetUrlToContent(Guid userId, Guid contentId, string assetUrl)
    {
        var content = await _dbContext.Contents
            .Include(c => c.Category)
            .Include(c => c.Tags)
            .FirstOrDefaultAsync(e => e.UserId == userId && e.ContentId == contentId);
        if (content == null) throw GeneralErrorCodes.NotFound;

        content.AssetUrl = assetUrl;

        await _dbContext.SaveChangesAsync();
        await IndexContentAsync(content);
        if (!string.IsNullOrEmpty(content.Slug))
        {
            await _cache.RemoveAsync(content.Slug);
        }
    }

    public async Task UpdateContentAssetUrl(Guid contentId, string assetUrl)
    {
        var content = await _dbContext.Contents
            .Include(c => c.Category)
            .Include(c => c.Tags)
            .FirstOrDefaultAsync(e => e.ContentId == contentId);
        if (content == null) throw GeneralErrorCodes.NotFound;

        content.AssetUrl = assetUrl;

        if (content.Status == "New")
        {
            content.Status = "Draft";
        }

        await _dbContext.SaveChangesAsync();
        await IndexContentAsync(content);
        if (!string.IsNullOrEmpty(content.Slug))
        {
            await _cache.RemoveAsync(content.Slug);
        }
    }

    public async Task<List<PublicContentDTO>> GetPublicContents(string? query, string? tag, string? category, DateTime? fromDate, DateTime? toDate, int page, int pageSize, bool withElastic = false, string? apiKey = null)
    {
        Guid? userId = null;
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            var key = await _apiKeyService.ValidateApiKeyAsync(apiKey);
            if (key != null)
            {
                userId = key.UserId;
            }
        }

        if (withElastic)
        {
            var response = await _elasticClient.SearchAsync<Content>(s => s
                .Indices(_elasticSettings.DefaultIndex)
                .From((page - 1) * pageSize)
                .Size(pageSize)
                .Sort(sort => sort.Field(f => f.CreatedOn, d => d.Order(SortOrder.Desc)))
                .Query(q => q
                    .Bool(b =>
                    {
                        var must = new List<Action<QueryDescriptor<Content>>>();

                        must.Add(m => m.Term(t => t.Field(f => f.Status.Suffix("keyword")).Value("Published")));

                        if (userId.HasValue)
                        {
                            must.Add(m => m.Term(t => t.Field(f => f.UserId).Value(userId.Value.ToString())));
                        }

                        if (!string.IsNullOrWhiteSpace(query))
                        {
                            must.Add(m => m.MultiMatch(mm => mm
                                .Fields(new [] { "title", "richContent" })
                                .Query(query)
                                .Fuzziness(new Fuzziness("AUTO"))
                            ));
                        }

                        if (!string.IsNullOrWhiteSpace(tag))
                        {
                            must.Add(m => m.Term(t => t.Field("tags.name.keyword").Value(tag)));
                        }

                        if (!string.IsNullOrWhiteSpace(category))
                        {
                            must.Add(m => m.Term(t => t.Field("category.name.keyword").Value(category)));
                        }

                        if (fromDate.HasValue)
                        {
                            must.Add(m => m.Range(r => r.Date(dr => dr.Field(f => f.CreatedOn).Gte(fromDate.Value))));
                        }

                        if (toDate.HasValue)
                        {
                            must.Add(m => m.Range(r => r.Date(dr => dr.Field(f => f.CreatedOn).Lte(toDate.Value))));
                        }

                        b.Must(must.ToArray());
                    })
                )
            );

            if (response.IsValidResponse)
            {
                return response.Documents.Select(c => new PublicContentDTO
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
                    Tags = c.Tags?.Select(t => new TagDTO
                    {
                        TagId = t.TagId,
                        Name = t.Name
                    }).ToList() ?? new List<TagDTO>()
                }).ToList();
            }

            _logger.LogError("Elastic search failed: {Reason}", response.DebugInformation);
        }

        var queryable = _dbContext.Contents
            .Include(c => c.Category)
            .Include(c => c.Tags)
            .Where(c => c.Status == "Published");

        if (!string.IsNullOrWhiteSpace(query))
        {
            queryable = queryable.Where(c => (c.Title != null && c.Title.Contains(query)) || (c.RichContent != null && c.RichContent.Contains(query)));
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

        if (userId.HasValue)
        {
            queryable = queryable.Where(c => c.UserId == userId.Value);
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
            var contentDocument = new
            {
                content.ContentId,
                content.Title,
                content.RichContent,
                content.Status,
                content.CreatedOn,
                content.UpdatedOn,
                content.AssetUrl,
                content.UserId,
                CategoryId = content.CategoryId,
                Category = content.Category == null ? null : new { content.Category.CategoryId, content.Category.Name, content.Category.Description },
                Tags = content.Tags.Select(t => new { t.TagId, t.Name })
            };

            var response = await _elasticClient.IndexAsync(contentDocument, i => i
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

    public async Task<PublicContentDTO> GetPublicContentBySlug(string slug, string apiKey)
    {
        var key = await _apiKeyService.ValidateApiKeyAsync(apiKey);
        if (key == null) throw GeneralErrorCodes.InvalidApiKey;

        string cacheKey = slug;
        string? cachedContent = await _cache.GetStringAsync(cacheKey);

        if (!string.IsNullOrEmpty(cachedContent))
        {
             var cachedDto = JsonSerializer.Deserialize<ContentDTO>(cachedContent)!;
             
             if (cachedDto.UserId != key.UserId)
             {
                 goto FetchFromDb;
             }

             if (cachedDto.Status != "Published")
             {
                 throw GeneralErrorCodes.NotFound;
             }
             
             return new PublicContentDTO
             {
                 ContentId = cachedDto.ContentId,
                 AssetUrl = cachedDto.AssetUrl,
                 Title = cachedDto.Title,
                 Slug = cachedDto.Slug,
                 RichContent = cachedDto.RichContent,
                 Status = cachedDto.Status,
                 CreatedOn = cachedDto.CreatedOn,
                 UpdatedOn = cachedDto.UpdatedOn,
                 UserId = cachedDto.UserId,
                 Category = cachedDto.CategoryId.HasValue ? new CategoryDTO 
                 { 
                     CategoryId = cachedDto.CategoryId.Value, 
                     Name = cachedDto.CategoryName ?? "",
                     Description = null 
                 } : null,
                 Tags = cachedDto.Tags
             };
        }

        FetchFromDb:
        var content = await _dbContext.Contents
            .Include(c => c.Category)
            .Include(c => c.Tags)
            .FirstOrDefaultAsync(c => c.Slug == slug && c.Status == "Published");

        if (content == null) throw GeneralErrorCodes.NotFound;
        
        if (content.UserId != key.UserId)
        {
             // Do not cache this 404 to prevent overwriting valid cache for the owner.
             throw GeneralErrorCodes.NotFound;
        }

        var contentDto = new ContentDTO
        {
            ContentId = content.ContentId,
            AssetUrl = content.AssetUrl,
            Status = content.Status,
            Title = content.Title,
            Slug = content.Slug,
            RichContent = content.RichContent,
            UserId = content.UserId,
            CategoryId = content.CategoryId,
            CategoryName = content.Category?.Name,
            CreatedOn = content.CreatedOn,
            UpdatedOn = content.UpdatedOn,
            Tags = content.Tags.Select(t => new TagDTO { TagId = t.TagId, Name = t.Name }).ToList()
        };

        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(contentDto), new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
        });

        return new PublicContentDTO
        {
            ContentId = content.ContentId,
            Title = content.Title,
            Slug = content.Slug,
            RichContent = content.RichContent,
            AssetUrl = content.AssetUrl,
            Status = content.Status,
            CreatedOn = content.CreatedOn,
            UpdatedOn = content.UpdatedOn,
            UserId = content.UserId,
            Category = content.Category == null ? null : new CategoryDTO
            {
                CategoryId = content.Category.CategoryId,
                Name = content.Category.Name,
                Description = content.Category.Description
            },
            Tags = content.Tags.Select(t => new TagDTO
            {
                TagId = t.TagId,
                Name = t.Name
            }).ToList()
        };
    }
}
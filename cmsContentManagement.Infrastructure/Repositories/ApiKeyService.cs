using cmsContentManagement.Application.Interfaces;
using cmsContentManagement.Domain.Entities;
using cmsContentManagment.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace cmsContentManagment.Infrastructure.Repositories;

public class ApiKeyService : IApiKeyService
{
    private readonly AppDbContext _context;

    public ApiKeyService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ApiKey> GenerateApiKeyAsync(Guid userId, string description)
    {
        var apiKey = new ApiKey
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Key = GenerateSecureKey(),
            Description = description,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.ApiKeys.Add(apiKey);
        await _context.SaveChangesAsync();

        return apiKey;
    }

    public async Task<bool> RevokeApiKeyAsync(Guid userId, Guid keyId)
    {
        var apiKey = await _context.ApiKeys
            .FirstOrDefaultAsync(k => k.Id == keyId && k.UserId == userId);

        if (apiKey == null) return false;

        _context.ApiKeys.Remove(apiKey);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<ApiKey?> ValidateApiKeyAsync(string key)
    {
        return await _context.ApiKeys
            .FirstOrDefaultAsync(k => k.Key == key && k.IsActive);
    }

    public async Task<List<ApiKey>> GetUserApiKeysAsync(Guid userId)
    {
        return await _context.ApiKeys
            .Where(k => k.UserId == userId)
            .ToListAsync();
    }

    private string GenerateSecureKey()
    {
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes).Replace("+", "").Replace("/", "").Replace("=", "");
    }
}

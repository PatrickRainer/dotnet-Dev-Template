using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyDevTemplate.Domain.Entities.ApiKeyAggregate;
using MyDevTemplate.Persistence;

namespace MyDevTemplate.Application.ApiKeyServices;

public class ApiKeyService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<ApiKeyService>? _logger;

    public ApiKeyService(AppDbContext dbContext, ILogger<ApiKeyService>? logger = null)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<ApiKeyRoot?> GetApiKeyAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbContext.ApiKeys.SingleOrDefaultAsync(a => a.Id == id, cancellationToken);
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Error getting API key by id {Id}", id);
            throw;
        }
    }

    public async Task<ApiKeyRoot?> ValidateApiKeyAsync(Guid tenantId, string key, CancellationToken cancellationToken = default)
    {
        try
        {
            // Note: Use IgnoreQueryFilters here because at authentication time, 
            // the TenantId is not yet available in the TenantProvider (it's being determined right now).
            return await _dbContext.ApiKeys
                .IgnoreQueryFilters()
                .SingleOrDefaultAsync(a => a.TenantId == tenantId && a.Key == key, cancellationToken);
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Error validating API key for tenant {TenantId}", tenantId);
            throw;
        }
    }

    public async Task<List<ApiKeyRoot>> GetApiKeysAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbContext.ApiKeys.ToListAsync(cancellationToken);
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Error getting API keys");
            throw;
        }
    }

    public async Task AddApiKeyAsync(ApiKeyRoot apiKey, CancellationToken cancellationToken = default)
    {
        try
        {
            await _dbContext.ApiKeys.AddAsync(apiKey, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Error adding API key");
            throw;
        }
    }

    public async Task UpdateApiKeyAsync(ApiKeyRoot apiKey, CancellationToken cancellationToken = default)
    {
        try
        {
            _dbContext.ApiKeys.Update(apiKey);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Error updating API key {Id}", apiKey.Id);
            throw;
        }
    }

    public async Task RemoveApiKeyAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var apiKey = await _dbContext.ApiKeys.SingleOrDefaultAsync(a => a.Id == id, cancellationToken);
            if (apiKey == null) throw new KeyNotFoundException($"API key with id {id} not found");

            _dbContext.ApiKeys.Remove(apiKey);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Error removing API key {Id}", id);
            throw;
        }
    }
}

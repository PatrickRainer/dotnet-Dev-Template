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

    public async Task<ApiKeyRootEntity?> GetApiKeyAsync(Guid id, CancellationToken cancellationToken = default)
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

    public async Task<ApiKeyRootEntity?> ValidateApiKeyAsync(string clientId, string key, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbContext.ApiKeys.SingleOrDefaultAsync(a => a.ClientId == clientId && a.Key == key, cancellationToken);
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Error validating API key for client {ClientId}", clientId);
            throw;
        }
    }

    public async Task<List<ApiKeyRootEntity>> GetApiKeysAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbContext.ApiKeys
                .Where(a => a.TenantId == tenantId)
                .ToListAsync(cancellationToken);
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Error getting API keys for tenant {TenantId}", tenantId);
            throw;
        }
    }

    public async Task AddApiKeyAsync(ApiKeyRootEntity apiKey, CancellationToken cancellationToken = default)
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

    public async Task UpdateApiKeyAsync(ApiKeyRootEntity apiKey, CancellationToken cancellationToken = default)
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

using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyDevTemplate.Application.Common.Interfaces;
using MyDevTemplate.Domain.Entities.ApiKeyAggregate;
using MyDevTemplate.Persistence;

namespace MyDevTemplate.Application.ApiKeyServices;

public class ApiKeyService : ICrudService<ApiKeyRoot, Guid>
{
    readonly AppDbContext _dbContext;
    readonly ILogger<ApiKeyService>? _logger;
    readonly IValidator<ApiKeyRoot> _validator;

    public ApiKeyService(AppDbContext dbContext, IValidator<ApiKeyRoot> validator, ILogger<ApiKeyService>? logger = null)
    {
        _dbContext = dbContext;
        _validator = validator;
        _logger = logger;
    }

    public async Task<ApiKeyRoot?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
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

    public async Task<List<ApiKeyRoot>> GetAllAsync(CancellationToken cancellationToken = default)
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

    public async Task<Guid> AddAsync(ApiKeyRoot apiKey, CancellationToken cancellationToken = default)
    {
        try
        {
            await _validator.ValidateAndThrowAsync(apiKey, cancellationToken);
            await _dbContext.ApiKeys.AddAsync(apiKey, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return apiKey.Id;
        }
        catch (ValidationException)
        {
            throw;
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Error adding API key");
            throw;
        }
    }

    public async Task UpdateAsync(ApiKeyRoot apiKey, CancellationToken cancellationToken = default)
    {
        try
        {
            await _validator.ValidateAndThrowAsync(apiKey, cancellationToken);
            _dbContext.ApiKeys.Update(apiKey);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (ValidationException)
        {
            throw;
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Error updating API key {Id}", apiKey.Id);
            throw;
        }
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
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

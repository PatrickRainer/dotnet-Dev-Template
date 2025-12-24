using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyDevTemplate.Application.Common.Interfaces;
using MyDevTemplate.Domain.Entities.TenantAggregate;
using MyDevTemplate.Domain.Contracts.Abstractions;
using MyDevTemplate.Persistence;

namespace MyDevTemplate.Application.TenantServices;

/// <summary>
/// Service for managing tenants. Access is restricted to the Master Tenant to ensure
/// that regular tenants cannot manage other tenants.
/// </summary>
public class TenantService : ICrudService<TenantRoot, Guid>
{
    readonly AppDbContext _dbContext;
    readonly ILogger<TenantService>? _logger;
    readonly IValidator<TenantRoot> _validator;
    readonly ITenantProvider _tenantProvider;

    public TenantService(AppDbContext dbContext, IValidator<TenantRoot> validator, ITenantProvider tenantProvider, ILogger<TenantService>? logger = null)
    {
        _dbContext = dbContext;
        _validator = validator;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    /// <summary>
    /// Verifies that the current caller is the Master Tenant.
    /// Throws UnauthorizedAccessException if not.
    /// </summary>
    private void EnsureMasterTenant()
    {
        if (!_tenantProvider.IsMasterTenant())
        {
            throw new UnauthorizedAccessException("Only the Master Tenant can use the TenantService.");
        }
    }

    public async Task<TenantRoot?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        EnsureMasterTenant();
        try
        {
            return await _dbContext.Tenants
                .IgnoreQueryFilters()
                .SingleOrDefaultAsync(t => t.Id == id, cancellationToken);
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Error getting tenant by id {TenantId}", id);
            throw;
        }
    }

    public async Task<List<TenantRoot>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        EnsureMasterTenant();
        try
        {
            return await _dbContext.Tenants
                .IgnoreQueryFilters()
                .ToListAsync(cancellationToken);
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Error getting all tenants");
            throw;
        }
    }

    public async Task<Guid> AddAsync(TenantRoot tenant, CancellationToken cancellationToken = default)
    {
        EnsureMasterTenant();
        try
        {
            await _validator.ValidateAndThrowAsync(tenant, cancellationToken);
            
            var result = await _dbContext.Tenants.AddAsync(tenant, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return result.Entity.Id;
        }
        catch (ValidationException)
        {
            throw;
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Error adding tenant {TenantName}", tenant.TenantName);
            throw;
        }
    }

    public async Task UpdateAsync(TenantRoot tenant, CancellationToken cancellationToken = default)
    {
        EnsureMasterTenant();
        try
        {
            await _validator.ValidateAndThrowAsync(tenant, cancellationToken);

            _dbContext.Tenants.Update(tenant);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (ValidationException)
        {
            throw;
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Error updating tenant {TenantId}", tenant.Id);
            throw;
        }
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        EnsureMasterTenant();
        try
        {
            var tenant = await _dbContext.Tenants
                .IgnoreQueryFilters()
                .SingleOrDefaultAsync(t => t.Id == id, cancellationToken);
            if (tenant == null)
            {
                _logger?.LogWarning("Tenant with id {TenantId} not found for deletion", id);
                return;
            }

            _dbContext.Tenants.Remove(tenant);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Error deleting tenant {TenantId}", id);
            throw;
        }
    }
}

using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyDevTemplate.Domain.Entities.TenantAggregate;
using MyDevTemplate.Persistence;

namespace MyDevTemplate.Application.TenantServices;

public class TenantService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<TenantService>? _logger;
    private readonly IValidator<TenantRoot> _validator;

    public TenantService(AppDbContext dbContext, IValidator<TenantRoot> validator, ILogger<TenantService>? logger = null)
    {
        _dbContext = dbContext;
        _validator = validator;
        _logger = logger;
    }

    public async Task<TenantRoot?> GetTenantByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
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

    public async Task<List<TenantRoot>> GetAllTenantsAsync(CancellationToken cancellationToken = default)
    {
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

    public async Task<Guid> AddTenantAsync(TenantRoot tenant, CancellationToken cancellationToken = default)
    {
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

    public async Task UpdateTenantAsync(TenantRoot tenant, CancellationToken cancellationToken = default)
    {
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

    public async Task DeleteTenantAsync(Guid id, CancellationToken cancellationToken = default)
    {
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

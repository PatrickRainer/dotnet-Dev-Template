using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyDevTemplate.Domain.Contracts.Abstractions;
using MyDevTemplate.Domain.Entities.SubscriptionAggregate;
using MyDevTemplate.Persistence;

namespace MyDevTemplate.Application.SubscriptionServices;

public class SubscriptionService : ISubscriptionService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<SubscriptionService>? _logger;
    private readonly IValidator<SubscriptionRoot> _validator;
    private readonly ITenantProvider _tenantProvider;

    public SubscriptionService(AppDbContext dbContext, IValidator<SubscriptionRoot> validator, ITenantProvider tenantProvider, ILogger<SubscriptionService>? logger = null)
    {
        _dbContext = dbContext;
        _validator = validator;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    private void EnsureMasterTenant()
    {
        if (!_tenantProvider.IsMasterTenant())
        {
            throw new UnauthorizedAccessException("Only the Master Tenant can manage Subscriptions.");
        }
    }

    public async Task<SubscriptionRoot?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbContext.Subscriptions.SingleOrDefaultAsync(s => s.Id == id, cancellationToken);
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Error getting subscription by id {SubscriptionId}", id);
            throw;
        }
    }

    public async Task<List<SubscriptionRoot>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbContext.Subscriptions.ToListAsync(cancellationToken);
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Error getting all subscriptions");
            throw;
        }
    }

    public async Task<Guid> AddAsync(SubscriptionRoot subscription, CancellationToken cancellationToken = default)
    {
        EnsureMasterTenant();
        try
        {
            await _validator.ValidateAndThrowAsync(subscription, cancellationToken);
            var result = await _dbContext.Subscriptions.AddAsync(subscription, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return result.Entity.Id;
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Error adding subscription {SubscriptionName}", subscription.Name);
            throw;
        }
    }

    public async Task UpdateAsync(SubscriptionRoot subscription, CancellationToken cancellationToken = default)
    {
        EnsureMasterTenant();
        try
        {
            await _validator.ValidateAndThrowAsync(subscription, cancellationToken);
            _dbContext.Subscriptions.Update(subscription);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Error updating subscription {SubscriptionId}", subscription.Id);
            throw;
        }
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        EnsureMasterTenant();
        try
        {
            var subscription = await _dbContext.Subscriptions.SingleOrDefaultAsync(s => s.Id == id, cancellationToken);
            if (subscription == null) return;

            _dbContext.Subscriptions.Remove(subscription);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Error deleting subscription {SubscriptionId}", id);
            throw;
        }
    }
}

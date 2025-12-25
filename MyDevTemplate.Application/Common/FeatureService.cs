using Microsoft.EntityFrameworkCore;
using MyDevTemplate.Application.Common.Interfaces;
using MyDevTemplate.Domain.Contracts.Abstractions;
using MyDevTemplate.Persistence;

namespace MyDevTemplate.Application.Common;

public class FeatureService : IFeatureService
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;
    private readonly IUserProvider _userProvider;

    public FeatureService(AppDbContext dbContext, ITenantProvider tenantProvider, IUserProvider userProvider)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
        _userProvider = userProvider;
    }

    public async Task<bool> HasFeatureAsync(string featureName)
    {
        if (_tenantProvider.IsMasterTenant())
        {
            return true; // Master tenant has access to all features
        }

        var tenantId = _tenantProvider.GetTenantId();
        if (tenantId == null)
        {
            return false;
        }

        // 1. Check Tenant's Subscription Model features
        var tenant = await _dbContext.Tenants
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == tenantId);

        if (tenant != null && tenant.SubscriptionId.HasValue)
        {
            var subscription = await _dbContext.Subscriptions
                .FirstOrDefaultAsync(s => s.Id == tenant.SubscriptionId.Value);
            if (subscription != null && subscription.Features.Contains(featureName))
            {
                return true;
            }
        }

        // 2. Check User's Role features
        var identityProviderId = _userProvider.GetIdentityProviderId();
        if (!string.IsNullOrEmpty(identityProviderId))
        {
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.IdentityProviderId == identityProviderId);

            if (user != null)
            {
                var roleIds = user.Roles.ToList();
                if (roleIds.Any())
                {
                    var roles = await _dbContext.Roles
                        .Where(r => roleIds.Contains(r.Id))
                        .ToListAsync();

                    if (roles.Any(r => r.Features.Contains(featureName)))
                    {
                        return true;
                    }
                }
            }
        }
        
        return false;
    }
}

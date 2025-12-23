using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using MyDevTemplate.Domain.Contracts.Abstractions;
using MyDevTemplate.Domain.Entities.Abstractions;
using MyDevTemplate.Domain.Entities.ApiKeyAggregate;
using MyDevTemplate.Domain.Entities.RoleAggregate;
using MyDevTemplate.Domain.Entities.TenantAggregate;
using MyDevTemplate.Domain.Entities.UserAggregate;

namespace MyDevTemplate.Persistence;

public class AppDbContext : DbContext
{
    readonly ITenantProvider _tenantProvider;

    public AppDbContext(DbContextOptions<AppDbContext> options, ITenantProvider tenantProvider) : base(options)
    {
        _tenantProvider = tenantProvider;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(EntityBase).IsAssignableFrom(entityType.ClrType) && entityType.ClrType != typeof(TenantRoot))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                
                // If Master Tenant, show all records (IsMasterTenant() returns true)
                // Otherwise, filter by TenantId
                var body = Expression.OrElse(
                    Expression.Property(Expression.Constant(this), nameof(IsMasterTenant)),
                    Expression.Equal(
                        Expression.Property(parameter, nameof(EntityBase.TenantId)),
                        Expression.Property(Expression.Constant(this), nameof(TenantProviderId))
                    )
                );
                
                var filter = Expression.Lambda(body, parameter);
                entityType.SetQueryFilter(filter);
            }
        }
    }

    Guid TenantProviderId => _tenantProvider.GetTenantId() ?? Guid.Empty;
    bool IsMasterTenant => _tenantProvider.IsMasterTenant();

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        var tenantId = _tenantProvider.GetTenantId();
        var isMasterTenant = _tenantProvider.IsMasterTenant();

        if (tenantId.HasValue)
        {
            foreach (var entry in ChangeTracker.Entries<EntityBase>())
            {
                if (entry.State == EntityState.Added && entry.Entity is not TenantRoot)
                {
                    // For Master Tenant, we only set TenantId if it's currently empty
                    // This allows Master Tenant to create records for other tenants
                    if (!isMasterTenant || entry.Entity.TenantId == Guid.Empty)
                    {
                        entry.Entity.TenantId = tenantId.Value;
                    }
                }
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }

    public DbSet<UserRoot> Users { get; set; }
    public DbSet<RoleRoot> Roles { get; set; }
    public DbSet<ApiKeyRootEntity> ApiKeys { get; set; }
    public DbSet<TenantRoot> Tenants { get; set; }
}
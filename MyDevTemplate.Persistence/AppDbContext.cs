using Microsoft.EntityFrameworkCore;
using MyDevTemplate.Domain.Entities.ApiKeyAggregate;
using MyDevTemplate.Domain.Entities.RoleAggregate;
using MyDevTemplate.Domain.Entities.TenantAggregate;
using MyDevTemplate.Domain.Entities.UserAggregate;

namespace MyDevTemplate.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    public DbSet<UserRootEntity> Users { get; set; }
    public DbSet<RoleRootEntity> Roles { get; set; }
    public DbSet<ApiKeyRootEntity> ApiKeys { get; set; }
    public DbSet<TenantRoot> Tenants { get; set; }
}
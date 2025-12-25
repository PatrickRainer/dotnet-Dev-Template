using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyDevTemplate.Domain.Entities.Common;
using MyDevTemplate.Persistence;
using Xunit;

namespace MyDevTemplate.Api.IntegrationTests;

public class SeedingTests : IntegrationTestBase
{
    public SeedingTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task Seeder_Should_Seed_Admin_Data_On_Startup()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Act
        // Seeding happens automatically on startup when Factory is created.

        // Assert
        var adminEmail = "patrick.rainer2@gmail.com";
        
        var masterTenant = await context.Tenants
            .FirstOrDefaultAsync(t => t.AdminEmail == adminEmail);
        Assert.NotNull(masterTenant);
        
        var role = await context.Roles.IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.Title == "TenantAdmin" && r.TenantId == masterTenant.Id);
        Assert.NotNull(role);
        
        var user = await context.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == new EmailAddress(adminEmail) && u.TenantId == masterTenant.Id);
        Assert.NotNull(user);
        Assert.Contains(role.Id, user.Roles);
        Assert.Contains(user.Id, role.Users);
    }
}

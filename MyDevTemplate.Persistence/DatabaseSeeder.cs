using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MyDevTemplate.Domain.Entities.Common;
using MyDevTemplate.Domain.Entities.RoleAggregate;
using MyDevTemplate.Domain.Entities.TenantAggregate;
using MyDevTemplate.Domain.Entities.UserAggregate;

namespace MyDevTemplate.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(AppDbContext context, IConfiguration configuration)
    {
        var masterTenantIdStr = configuration["Authentication:TenantId"];
        if (!Guid.TryParse(masterTenantIdStr, out var masterTenantId))
        {
            if (!Guid.TryParse(configuration.GetSection("MasterTenant")["Id"], out masterTenantId))
            {
                throw new Exception("Master Tenant Id not found in configuration");
            }
        }

        var adminEmail = configuration["Authentication:AdminEmail"];
        if (string.IsNullOrWhiteSpace(adminEmail))
        {
            return;
        }

        // 1. Seed Master Tenant
        var masterTenant = await context.Tenants.AsTracking().FirstOrDefaultAsync(t => t.Id == masterTenantId);
        if (masterTenant == null)
        {
            masterTenant = new TenantRoot("Master Tenant", "Master Company", adminEmail);
            masterTenant.Id = masterTenantId;
            context.Tenants.Add(masterTenant);
            await context.SaveChangesAsync();
        }

        // 2. Seed TenantAdmin Role
        var roleName = "TenantAdmin";
        var tenantAdminRole = await context.Roles.IgnoreQueryFilters().AsTracking()
            .FirstOrDefaultAsync(r => r.Title == roleName && r.TenantId == masterTenantId);
        
        if (tenantAdminRole == null)
        {
            tenantAdminRole = new RoleRoot(roleName, "Administrator role for the tenant");
            tenantAdminRole.TenantId = masterTenantId;
            context.Roles.Add(tenantAdminRole);
            await context.SaveChangesAsync();
        }

        // 3. Seed Admin User
        var userEmail = new EmailAddress(adminEmail);
        var adminUser = await context.Users.IgnoreQueryFilters().AsTracking()
            .FirstOrDefaultAsync(u => u.Email == userEmail && u.TenantId == masterTenantId);

        if (adminUser == null)
        {
            adminUser = new UserRoot(userEmail, "Admin", "User", "seed-admin-id");
            adminUser.TenantId = masterTenantId;
            adminUser.AddRole(tenantAdminRole.Id);
            context.Users.Add(adminUser);
            
            // Also add user to role
            tenantAdminRole.AddUser(adminUser.Id);
            context.Roles.Update(tenantAdminRole);
            
            await context.SaveChangesAsync();
        }
        else
        {
            bool changed = false;
            // Ensure user has the role
            if (!adminUser.Roles.Contains(tenantAdminRole.Id))
            {
                adminUser.AddRole(tenantAdminRole.Id);
                context.Users.Update(adminUser);
                changed = true;
            }
            
            // Ensure role has the user
            if (!tenantAdminRole.Users.Contains(adminUser.Id))
            {
                tenantAdminRole.AddUser(adminUser.Id);
                context.Roles.Update(tenantAdminRole);
                changed = true;
            }
            
            if (changed)
            {
                await context.SaveChangesAsync();
            }
        }
    }
}

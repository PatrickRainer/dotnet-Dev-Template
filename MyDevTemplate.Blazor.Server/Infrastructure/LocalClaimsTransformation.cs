using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using MyDevTemplate.Persistence;

namespace MyDevTemplate.Blazor.Server.Infrastructure;

public class LocalClaimsTransformation : IClaimsTransformation
{
    private readonly IServiceProvider _serviceProvider;

    public LocalClaimsTransformation(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        // Check if the user is authenticated
        if (principal.Identity is not { IsAuthenticated: true })
        {
            return principal;
        }

        if (principal.HasClaim(c => c.Type == "TenantId"))
        {
            return principal;
        }

        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var oid = principal.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;
        if (string.IsNullOrEmpty(oid))
        {
            return principal;
        }

        var user = await dbContext.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.IdentityProviderId == oid);

        if (user == null)
        {
            return principal;
        }

        // Clone the identity so we don't mutate the original
        var clone = principal.Clone();
        var newIdentity = (ClaimsIdentity)clone.Identity!;

        // Add TenantId claim
        newIdentity.AddClaim(new Claim("TenantId", user.TenantId.ToString()));

        // Add Role claims
        var roles = await dbContext.Roles
            .IgnoreQueryFilters()
            .Where(r => user.Roles.Contains(r.Id))
            .ToListAsync();

        foreach (var role in roles)
        {
            newIdentity.AddClaim(new Claim(ClaimTypes.Role, role.Title));
        }

        return clone;
    }
}
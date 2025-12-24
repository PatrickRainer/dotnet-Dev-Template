using System.Security.Claims;
using MyDevTemplate.Domain.Contracts.Abstractions;

namespace MyDevTemplate.Blazor.Server.Infrastructure;

public class BlazorTenantProvider : ITenantProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public BlazorTenantProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? GetTenantId()
    {
        var tenantIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("TenantId")?.Value;

        if (Guid.TryParse(tenantIdClaim, out var tenantId))
        {
            return tenantId;
        }

        return null;
    }

    public bool IsMasterTenant()
    {
        // Adjust this logic if needed for Blazor
        return _httpContextAccessor.HttpContext?.User?.Identity?.Name == "MasterKeyUser";
    }
}

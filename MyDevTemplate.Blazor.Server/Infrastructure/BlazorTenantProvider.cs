using System.Security.Claims;
using MyDevTemplate.Domain.Contracts.Abstractions;

namespace MyDevTemplate.Blazor.Server.Infrastructure;

public class BlazorTenantProvider : ITenantProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;

    public BlazorTenantProvider(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
    {
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
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
        var tenantId = GetTenantId();
        if (tenantId == null) return false;

        var masterTenantId = GetMasterTenantId();
        if (masterTenantId != null)
        {
            return tenantId == masterTenantId;
        }

        return false;
    }

    public Guid? GetMasterTenantId()
    {
        var masterTenantIdStr = _configuration["Authentication:TenantId"];
        if (Guid.TryParse(masterTenantIdStr, out var masterTenantId))
        {
            return masterTenantId;
        }

        return null;
    }
}

using Microsoft.Extensions.Configuration;
using MyDevTemplate.Domain.Contracts.Abstractions;

namespace MyDevTemplate.Api.Providers;

public class HttpTenantProvider : ITenantProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;

    public HttpTenantProvider(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
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
        return _httpContextAccessor.HttpContext?.User?.Identity?.Name == "MasterKeyUser";
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

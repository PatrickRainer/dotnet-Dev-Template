using MyDevTemplate.Domain.Contracts.Abstractions;

namespace MyDevTemplate.Api.Providers;

public class HttpTenantProvider : ITenantProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpTenantProvider(IHttpContextAccessor httpContextAccessor)
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
        return _httpContextAccessor.HttpContext?.User?.Identity?.Name == "MasterKeyUser";
    }
}

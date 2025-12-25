using MyDevTemplate.Domain.Contracts.Abstractions;

namespace MyDevTemplate.Api.Providers;

public class HttpUserProvider : IUserProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpUserProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? GetIdentityProviderId()
    {
        return _httpContextAccessor.HttpContext?.User?.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;
    }
}

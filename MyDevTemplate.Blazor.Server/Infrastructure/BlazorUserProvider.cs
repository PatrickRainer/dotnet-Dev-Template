using MyDevTemplate.Domain.Contracts.Abstractions;

namespace MyDevTemplate.Blazor.Server.Infrastructure;

public class BlazorUserProvider : IUserProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public BlazorUserProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? GetIdentityProviderId()
    {
        return _httpContextAccessor.HttpContext?.User?.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;
    }
}

using Microsoft.AspNetCore.Authentication;

namespace MyDevTemplate.Api.Authentication;

public static class AuthenticationServiceCollectionExtensions
{
    public static IServiceCollection AddApiKeyAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(ApiKeyConstants.AuthenticationScheme)
            .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(
                ApiKeyConstants.AuthenticationScheme, null);

        services.AddAuthorization();

        return services;
    }
}

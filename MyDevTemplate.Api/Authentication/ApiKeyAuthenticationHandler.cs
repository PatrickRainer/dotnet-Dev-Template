using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using MyDevTemplate.Application.ApiKeyServices;
using Microsoft.Extensions.DependencyInjection;

namespace MyDevTemplate.Api.Authentication;

public class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IConfiguration _configuration;

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IConfiguration configuration)
        : base(options, logger, encoder)
    {
        _configuration = configuration;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(ApiKeyConstants.HeaderName, out var extractedApiKey))
        {
            return AuthenticateResult.NoResult();
        }
        if (!Request.Headers.TryGetValue(ApiKeyConstants.TenantIdHeaderName, out var extractedTenantId))
        {
            return AuthenticateResult.Fail("Tenant ID is missing");
        }

        if (!Guid.TryParse(extractedTenantId, out var tenantId))
        {
            return AuthenticateResult.Fail("Invalid Tenant ID format");
        }

        var expectedApiKey = _configuration.GetValue<string>("Authentication:ApiKey");

        // Check if it's the master key
        if (!string.IsNullOrEmpty(expectedApiKey) && extractedApiKey == expectedApiKey)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, "MasterKeyUser"),
                new Claim("TenantId", extractedTenantId.ToString())
            };

            return Success(claims);
        }

        // Check database for dynamic keys
        var apiKeyService = Context.RequestServices.GetRequiredService<ApiKeyService>();
        var apiKeyEntity = await apiKeyService.ValidateApiKeyAsync(tenantId, extractedApiKey!);

        if (apiKeyEntity != null && apiKeyEntity.IsValid)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, apiKeyEntity.Label),
                new Claim("TenantId", apiKeyEntity.TenantId.ToString())
            };

            return Success(claims);
        }

        return AuthenticateResult.Fail("Invalid API Key");
    }

    private AuthenticateResult Success(Claim[] claims)
    {
        var identity = new ClaimsIdentity(claims, ApiKeyConstants.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, ApiKeyConstants.AuthenticationScheme);

        return AuthenticateResult.Success(ticket);
    }
}

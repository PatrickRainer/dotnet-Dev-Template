using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

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

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(ApiKeyConstants.HeaderName, out var extractedApiKey))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var expectedApiKey = _configuration.GetValue<string>("Authentication:ApiKey");

        if (string.IsNullOrEmpty(expectedApiKey) || extractedApiKey != expectedApiKey)
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid API Key"));
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "ApiKeyUser")
        };

        var identity = new ClaimsIdentity(claims, ApiKeyConstants.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, ApiKeyConstants.AuthenticationScheme);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

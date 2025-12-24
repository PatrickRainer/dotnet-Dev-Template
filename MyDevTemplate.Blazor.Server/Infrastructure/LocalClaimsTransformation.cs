using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace MyDevTemplate.Blazor.Server.Infrastructure;

public class LocalClaimsTransformation : IClaimsTransformation
{

    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        // Check if the user is authenticated
        if (principal.Identity is not { IsAuthenticated: true })
        {
            return Task.FromResult(principal);
        }

        // Clone the identity so we don't mutate the original if we don't want to
        var clone = principal.Clone();
        var newIdentity = (ClaimsIdentity)clone.Identity!;

        // Catch and inspect claims here
        var email = principal.FindFirst(ClaimTypes.Email)?.Value 
                    ?? principal.FindFirst("preferred_username")?.Value;

        // Example: Add a custom claim or perform logic based on existing ones
        if (!principal.HasClaim(c => c.Type == "MyCustomClaim"))
        {
            newIdentity.AddClaim(new Claim("MyCustomClaim", "ProcessedTest"));
        }

        return Task.FromResult(clone);
    }

}
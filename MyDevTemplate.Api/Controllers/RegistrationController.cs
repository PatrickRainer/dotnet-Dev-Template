using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyDevTemplate.Application.RegistrationServices;
using MyDevTemplate.Application.RegistrationServices.Dtos;

namespace MyDevTemplate.Api.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
public class RegistrationController : ControllerBase
{
    private readonly IRegistrationService _registrationService;
    private readonly ILogger<RegistrationController> _logger;

    public RegistrationController(IRegistrationService registrationService, ILogger<RegistrationController> logger)
    {
        _registrationService = registrationService;
        _logger = logger;
    }

    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Register([FromBody] RegistrationDto registrationDto, [FromHeader(Name = "X-Admin-Email")] string? adminEmail, CancellationToken cancellationToken)
    {
        try
        {
            // If the user is authenticated (e.g. via API Key or OIDC), we use the identity name.
            // For Blazor Server, it's already authenticated when reaching this via service call (indirectly).
            // For external API calls, we allow passing the admin email in the header if it's a new registration.
            
            var effectiveAdminEmail = User.Identity?.Name ?? adminEmail;
            
            if (string.IsNullOrEmpty(effectiveAdminEmail))
            {
                return BadRequest("Admin email is required for registration. Either authenticate or provide X-Admin-Email header.");
            }

            var result = await _registrationService.RegisterAsync(registrationDto, effectiveAdminEmail, cancellationToken);
            return Ok(new { TenantId = result });
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during company registration");
            return StatusCode(500, "Internal server error");
        }
    }
}

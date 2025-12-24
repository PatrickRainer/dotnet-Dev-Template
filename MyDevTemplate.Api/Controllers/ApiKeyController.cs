using FluentValidation;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyDevTemplate.Application.ApiKeyServices;
using MyDevTemplate.Application.ApiKeyServices.Dtos;
using MyDevTemplate.Domain.Entities.ApiKeyAggregate;

namespace MyDevTemplate.Api.Controllers;

[Authorize]
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public class ApiKeyController : ControllerBase
{
    private readonly ApiKeyService _apiKeyService;
    private readonly ILogger<ApiKeyController>? _logger;
    private readonly IValidator<AddApiKeyDto> _addValidator;

    public ApiKeyController(
        ApiKeyService apiKeyService, 
        IValidator<AddApiKeyDto> addValidator,
        ILogger<ApiKeyController>? logger = null)
    {
        _apiKeyService = apiKeyService;
        _addValidator = addValidator;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ApiKeyRoot>))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ApiKeyRoot>>> GetApiKeys(CancellationToken cancellationToken)
    {
        try
        {
            // Note: tenantId is now automatically handled by AppDbContext global filter
            var apiKeys = await _apiKeyService.GetApiKeysAsync(Guid.Empty, cancellationToken);
            return Ok(apiKeys);
        }
        catch (OperationCanceledException)
        {
            return StatusCode(499);
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Error getting API keys");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Guid))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Guid>> AddApiKey([FromBody] AddApiKeyDto addApiKeyDto, CancellationToken cancellationToken)
    {
        try
        {
            await _addValidator.ValidateAndThrowAsync(addApiKeyDto, cancellationToken);
            
            var apiKey = new ApiKeyRoot(addApiKeyDto.Key, addApiKeyDto.Label, addApiKeyDto.ExpiresAtUtc);

            if (!string.IsNullOrEmpty(addApiKeyDto.TenantId) && Guid.TryParse(addApiKeyDto.TenantId, out var tenantGuid))
            {
                apiKey.TenantId = tenantGuid;
            }

            await _apiKeyService.AddApiKeyAsync(apiKey, cancellationToken);

            return Ok(apiKey.Id);
        }
        catch (OperationCanceledException)
        {
            return StatusCode(499);
        }
        catch (ValidationException e)
        {
            return BadRequest(e.Errors);
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
        }
        catch (Exception e) when (e.Message.Contains("duplicate key value violates unique constraint") || e.InnerException?.Message.Contains("duplicate key value violates unique constraint") == true || e.InnerException?.Message.Contains("Cannot insert duplicate key row") == true)
        {
            return Conflict("API key already exists");
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Error adding API key");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<string>> RemoveApiKey(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _apiKeyService.RemoveApiKeyAsync(id, cancellationToken);
            return Ok("API key removed successfully");
        }
        catch (OperationCanceledException)
        {
            return StatusCode(499);
        }
        catch (KeyNotFoundException)
        {
            return NotFound("API key not found");
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Error removing API key");
            return StatusCode(500, "Internal server error");
        }
    }
}

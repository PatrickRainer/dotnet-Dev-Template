using System.ComponentModel.DataAnnotations;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyDevTemplate.Application.ApiKeyServices;
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

    public ApiKeyController(ApiKeyService apiKeyService, ILogger<ApiKeyController>? logger = null)
    {
        _apiKeyService = apiKeyService;
        _logger = logger;
    }

    [HttpGet("tenant/{tenantId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ApiKeyRootEntity>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ApiKeyRootEntity>>> GetApiKeys(string tenantId, CancellationToken cancellationToken)
    {
        try
        {
            if (!Guid.TryParse(tenantId, out var tenantGuid))
            {
                return BadRequest("Invalid TenantId format. Must be a Guid.");
            }

            var apiKeys = await _apiKeyService.GetApiKeysAsync(tenantGuid, cancellationToken);
            return Ok(apiKeys);
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Error getting API keys");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Guid))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Guid>> AddApiKey([FromBody] AddApiKeyDto addApiKeyDto, CancellationToken cancellationToken)
    {
        try
        {
            if (!Guid.TryParse(addApiKeyDto.TenantId, out var tenantGuid))
            {
                return BadRequest("Invalid TenantId format. Must be a Guid.");
            }

            var apiKey = new ApiKeyRootEntity(addApiKeyDto.Key, addApiKeyDto.Label, addApiKeyDto.ExpiresAtUtc)
            {
                TenantId = tenantGuid
            };

            await _apiKeyService.AddApiKeyAsync(apiKey, cancellationToken);

            return Ok(apiKey.Id);
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

public record AddApiKeyDto(
    [Required] string Key,
    [Required] string Label,
    [Required] string TenantId,
    DateTime? ExpiresAtUtc);

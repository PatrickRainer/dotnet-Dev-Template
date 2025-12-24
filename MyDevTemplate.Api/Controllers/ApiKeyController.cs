using FluentValidation;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyDevTemplate.Application.ApiKeyServices;
using MyDevTemplate.Application.ApiKeyServices.Dtos;
using MyDevTemplate.Domain.Contracts.Abstractions;
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
    private readonly IValidator<UpdateApiKeyDto> _updateValidator;
    private readonly ITenantProvider _tenantProvider;

    public ApiKeyController(
        ApiKeyService apiKeyService, 
        IValidator<AddApiKeyDto> addValidator,
        IValidator<UpdateApiKeyDto> updateValidator,
        ITenantProvider tenantProvider,
        ILogger<ApiKeyController>? logger = null)
    {
        _apiKeyService = apiKeyService;
        _addValidator = addValidator;
        _updateValidator = updateValidator;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ApiKeyRoot>))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ApiKeyRoot>>> GetApiKeys(CancellationToken cancellationToken)
    {
        try
        {
            var apiKeys = await _apiKeyService.GetAllAsync(cancellationToken);
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

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiKeyRoot))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiKeyRoot>> GetApiKey(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var apiKey = await _apiKeyService.GetByIdAsync(id, cancellationToken);
            if (apiKey == null)
            {
                return NotFound();
            }

            return Ok(apiKey);
        }
        catch (OperationCanceledException)
        {
            return StatusCode(499);
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Error getting API key with id {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Adds a new API key. If the caller is a regular tenant, the API key will be automatically 
    /// associated with their tenant, even if they provide a different TenantId in the request.
    /// Master tenants can explicitly specify a TenantId to create keys for other tenants.
    /// </summary>
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
            else
            {
                var currentTenantId = _tenantProvider.GetTenantId();
                if (currentTenantId.HasValue)
                {
                    apiKey.TenantId = currentTenantId.Value;
                }
            }

            await _apiKeyService.AddAsync(apiKey, cancellationToken);

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

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<string>> UpdateApiKey(Guid id, [FromBody] UpdateApiKeyDto updateApiKeyDto, CancellationToken cancellationToken)
    {
        try
        {
            await _updateValidator.ValidateAndThrowAsync(updateApiKeyDto, cancellationToken);
            
            var apiKey = await _apiKeyService.GetByIdAsync(id, cancellationToken);
            if (apiKey == null)
            {
                return NotFound("API key not found");
            }

            apiKey.Label = updateApiKeyDto.Label;
            apiKey.IsActive = updateApiKeyDto.IsActive;
            apiKey.ExpiresAtUtc = updateApiKeyDto.ExpiresAtUtc;

            await _apiKeyService.UpdateAsync(apiKey, cancellationToken);

            return Ok("API key updated successfully");
        }
        catch (OperationCanceledException)
        {
            return StatusCode(499);
        }
        catch (ValidationException e)
        {
            return BadRequest(e.Errors);
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Error updating API key with id {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<string>> RemoveApiKey(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _apiKeyService.DeleteAsync(id, cancellationToken);
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

using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyDevTemplate.Application.TenantServices;
using MyDevTemplate.Application.TenantServices.Dtos;
using MyDevTemplate.Domain.Entities.TenantAggregate;

namespace MyDevTemplate.Api.Controllers;

[Authorize(Policy = "MasterTenant")]
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public class TenantController : ControllerBase
{
    private readonly ITenantService _tenantService;
    private readonly ILogger<TenantController>? _logger;
    private readonly IValidator<CreateTenantDto> _createValidator;
    private readonly IValidator<UpdateTenantDto> _updateValidator;

    public TenantController(
        ITenantService tenantService, 
        IValidator<CreateTenantDto> createValidator,
        IValidator<UpdateTenantDto> updateValidator,
        ILogger<TenantController>? logger = null)
    {
        _tenantService = tenantService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<TenantRoot>))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<TenantRoot>>> GetAllTenants(CancellationToken cancellationToken)
    {
        try
        {
            var tenants = await _tenantService.GetAllAsync(cancellationToken);
            return Ok(tenants);
        }
        catch (OperationCanceledException)
        {
            return StatusCode(499);
        }
        catch (UnauthorizedAccessException e)
        {
            return Forbid(e.Message);
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Error getting all tenants");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TenantRoot))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TenantRoot>> GetTenant(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var tenant = await _tenantService.GetByIdAsync(id, cancellationToken);
            if (tenant == null)
            {
                return NotFound();
            }

            return Ok(tenant);
        }
        catch (OperationCanceledException)
        {
            return StatusCode(499);
        }
        catch (UnauthorizedAccessException e)
        {
            return Forbid(e.Message);
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Error getting tenant with id {TenantId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(TenantRoot))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TenantRoot>> CreateTenant([FromBody] CreateTenantDto createTenantDto, CancellationToken cancellationToken)
    {
        try
        {
            await _createValidator.ValidateAndThrowAsync(createTenantDto, cancellationToken);
            
            var tenant = new TenantRoot(createTenantDto.TenantName, createTenantDto.CompanyName,
                createTenantDto.AdminEmail, createTenantDto.SubscriptionId);
            tenant.AddAddress(
                createTenantDto.Street ?? string.Empty,
                createTenantDto.City ?? string.Empty,
                createTenantDto.State ?? string.Empty,
                createTenantDto.Country ?? string.Empty,
                createTenantDto.ZipCode ?? string.Empty);

            await _tenantService.AddAsync(tenant, cancellationToken);

            return CreatedAtAction(nameof(GetTenant), new { id = tenant.Id }, tenant);
        }
        catch (OperationCanceledException)
        {
            return StatusCode(499);
        }
        catch (UnauthorizedAccessException e)
        {
            return Forbid(e.Message);
        }
        catch (ValidationException e)
        {
            return BadRequest(e.Errors);
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Error creating tenant");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateTenant(Guid id, [FromBody] UpdateTenantDto updateTenantDto, CancellationToken cancellationToken)
    {
        try
        {
            await _updateValidator.ValidateAndThrowAsync(updateTenantDto, cancellationToken);
            
            var tenant = await _tenantService.GetByIdAsync(id, cancellationToken);
            if (tenant == null)
            {
                return NotFound();
            }

            tenant.TenantName = updateTenantDto.TenantName;
            tenant.CompanyName = updateTenantDto.CompanyName;
            tenant.SubscriptionId = updateTenantDto.SubscriptionId;
            tenant.AddAddress(
                updateTenantDto.Street ?? string.Empty,
                updateTenantDto.City ?? string.Empty,
                updateTenantDto.State ?? string.Empty,
                updateTenantDto.Country ?? string.Empty,
                updateTenantDto.ZipCode ?? string.Empty);

            await _tenantService.UpdateAsync(tenant, cancellationToken);

            return NoContent();
        }
        catch (OperationCanceledException)
        {
            return StatusCode(499);
        }
        catch (UnauthorizedAccessException e)
        {
            return Forbid(e.Message);
        }
        catch (ValidationException e)
        {
            return BadRequest(e.Errors);
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Error updating tenant with id {TenantId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteTenant(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var tenant = await _tenantService.GetByIdAsync(id, cancellationToken);
            if (tenant == null)
            {
                return NotFound();
            }

            await _tenantService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
        catch (OperationCanceledException)
        {
            return StatusCode(499);
        }
        catch (UnauthorizedAccessException e)
        {
            return Forbid(e.Message);
        }
        catch (InvalidOperationException e)
        {
            return BadRequest(e.Message);
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Error deleting tenant with id {TenantId}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}


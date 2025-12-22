using System.ComponentModel.DataAnnotations;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyDevTemplate.Application.TenantServices;
using MyDevTemplate.Domain.Entities.Common;
using MyDevTemplate.Domain.Entities.TenantAggregate;

namespace MyDevTemplate.Api.Controllers;

[Authorize(Policy = "MasterTenant")]
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public class TenantController : ControllerBase
{
    private readonly TenantService _tenantService;
    private readonly ILogger<TenantController>? _logger;

    public TenantController(TenantService tenantService, ILogger<TenantController>? logger = null)
    {
        _tenantService = tenantService;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<TenantRoot>))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<TenantRoot>>> GetAllTenants(CancellationToken cancellationToken)
    {
        try
        {
            var tenants = await _tenantService.GetAllTenantsAsync(cancellationToken);
            return Ok(tenants);
        }
        catch (OperationCanceledException)
        {
            return StatusCode(499);
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
            var tenant = await _tenantService.GetTenantByIdAsync(id, cancellationToken);
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
            var tenant = new TenantRoot(createTenantDto.TenantName, createTenantDto.CompanyName)
            {
                Address = new Address(
                    createTenantDto.Street ?? string.Empty,
                    createTenantDto.City ?? string.Empty,
                    createTenantDto.State ?? string.Empty,
                    createTenantDto.Country ?? string.Empty,
                    createTenantDto.ZipCode ?? string.Empty)
            };

            await _tenantService.AddTenantAsync(tenant, cancellationToken);

            return CreatedAtAction(nameof(GetTenant), new { id = tenant.Id }, tenant);
        }
        catch (OperationCanceledException)
        {
            return StatusCode(499);
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
            var tenant = await _tenantService.GetTenantByIdAsync(id, cancellationToken);
            if (tenant == null)
            {
                return NotFound();
            }

            tenant.TenantName = updateTenantDto.TenantName;
            tenant.CompanyName = updateTenantDto.CompanyName;
            tenant.Address = new Address(
                updateTenantDto.Street ?? string.Empty,
                updateTenantDto.City ?? string.Empty,
                updateTenantDto.State ?? string.Empty,
                updateTenantDto.Country ?? string.Empty,
                updateTenantDto.ZipCode ?? string.Empty);

            await _tenantService.UpdateTenantAsync(tenant, cancellationToken);

            return NoContent();
        }
        catch (OperationCanceledException)
        {
            return StatusCode(499);
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Error updating tenant with id {TenantId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteTenant(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var tenant = await _tenantService.GetTenantByIdAsync(id, cancellationToken);
            if (tenant == null)
            {
                return NotFound();
            }

            await _tenantService.DeleteTenantAsync(id, cancellationToken);
            return NoContent();
        }
        catch (OperationCanceledException)
        {
            return StatusCode(499);
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Error deleting tenant with id {TenantId}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}

public record CreateTenantDto(
    [Required] string TenantName,
    [Required] string CompanyName,
    string? Street,
    string? City,
    string? State,
    string? Country,
    string? ZipCode);

public record UpdateTenantDto(
    [Required] string TenantName,
    [Required] string CompanyName,
    string? Street,
    string? City,
    string? State,
    string? Country,
    string? ZipCode);

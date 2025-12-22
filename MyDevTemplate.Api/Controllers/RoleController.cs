using System.ComponentModel.DataAnnotations;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyDevTemplate.Application.RoleServices;
using MyDevTemplate.Domain.Entities.RoleAggregate;

namespace MyDevTemplate.Api.Controllers;

[Authorize]
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public class RoleController : ControllerBase
{
    private readonly ILogger<RoleController>? _logger;
    private readonly RoleService _roleService;

    public RoleController(RoleService roleService, ILogger<RoleController>? logger = null)
    {
        _roleService = roleService;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<RoleRootEntity>))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<RoleRootEntity>>> GetRoles(CancellationToken cancellationToken)
    {
        try
        {
            var roles = await _roleService.GetAllRolesAsync(cancellationToken);
            return Ok(roles);
        }
        catch (OperationCanceledException)
        {
            return StatusCode(499);
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Error getting roles");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RoleRootEntity))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RoleRootEntity>> GetRole([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var role = await _roleService.GetRoleByIdAsync(id, cancellationToken);
            if (role == null)
            {
                return NotFound();
            }

            return Ok(role);
        }
        catch (OperationCanceledException)
        {
            return StatusCode(499);
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Error getting role");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Guid))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Guid>> AddRole([FromBody] AddRoleDto addRoleDto, CancellationToken cancellationToken)
    {
        try
        {
            var role = new RoleRootEntity(addRoleDto.Title, addRoleDto.Description);

            await _roleService.AddRoleAsync(role, cancellationToken);

            return Ok(role.Id);
        }
        catch (OperationCanceledException)
        {
            return StatusCode(499);
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Error adding role");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<string>> UpdateRole([FromRoute] Guid id, [FromBody] UpdateRoleDto updateRoleDto, CancellationToken cancellationToken)
    {
        try
        {
            var role = await _roleService.GetRoleByIdAsync(id, cancellationToken);
            if (role == null)
            {
                return NotFound("Role not found");
            }

            role.Title = updateRoleDto.Title;
            role.Description = updateRoleDto.Description;

            await _roleService.UpdateRoleAsync(role, cancellationToken);

            return Ok("Role updated successfully");
        }
        catch (OperationCanceledException)
        {
            return StatusCode(499);
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Error updating role");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<string>> RemoveRole([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _roleService.RemoveRoleAsync(id, cancellationToken);
            return Ok("Role removed successfully");
        }
        catch (OperationCanceledException)
        {
            return StatusCode(499);
        }
        catch (KeyNotFoundException)
        {
            return NotFound("Role not found");
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Error removing role");
            return StatusCode(500, "Internal server error");
        }
    }
}

public record AddRoleDto(
    [Required] string Title,
    string Description);

public record UpdateRoleDto(
    [Required] string Title,
    string Description);

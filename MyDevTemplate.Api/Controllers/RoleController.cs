using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyDevTemplate.Application.RoleServices;
using MyDevTemplate.Application.RoleServices.Dtos;
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
    private readonly IRoleService _roleService;
    private readonly IValidator<AddRoleDto> _addValidator;
    private readonly IValidator<UpdateRoleDto> _updateValidator;

    public RoleController(
        IRoleService roleService, 
        IValidator<AddRoleDto> addValidator,
        IValidator<UpdateRoleDto> updateValidator,
        ILogger<RoleController>? logger = null)
    {
        _roleService = roleService;
        _addValidator = addValidator;
        _updateValidator = updateValidator;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<RoleRoot>))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<RoleRoot>>> GetRoles(CancellationToken cancellationToken)
    {
        try
        {
            var roles = await _roleService.GetAllAsync(cancellationToken);
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
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RoleRoot))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RoleRoot>> GetRole([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var role = await _roleService.GetByIdAsync(id, cancellationToken);
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
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Guid>> AddRole([FromBody] AddRoleDto addRoleDto, CancellationToken cancellationToken)
    {
        try
        {
            await _addValidator.ValidateAndThrowAsync(addRoleDto, cancellationToken);
            
            var role = new RoleRoot(addRoleDto.Title, addRoleDto.Description);

            await _roleService.AddAsync(role, cancellationToken);

            return Ok(role.Id);
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
        catch (Exception e)
        {
            _logger?.LogError(e, "Error adding role");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<string>> UpdateRole([FromRoute] Guid id, [FromBody] UpdateRoleDto updateRoleDto, CancellationToken cancellationToken)
    {
        try
        {
            await _updateValidator.ValidateAndThrowAsync(updateRoleDto, cancellationToken);
            
            var role = await _roleService.GetByIdAsync(id, cancellationToken);
            if (role == null)
            {
                return NotFound("Role not found");
            }

            role.Title = updateRoleDto.Title;
            role.Description = updateRoleDto.Description;

            await _roleService.UpdateAsync(role, cancellationToken);

            return Ok("Role updated successfully");
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
            await _roleService.DeleteAsync(id, cancellationToken);
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

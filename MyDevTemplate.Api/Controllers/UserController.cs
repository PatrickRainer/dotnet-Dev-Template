using System.ComponentModel.DataAnnotations;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using MyDevTemplate.Application.UserServices;
using MyDevTemplate.Domain.Entities.Common;
using MyDevTemplate.Domain.Entities.UserAggregate;

namespace MyDevTemplate.Api.Controllers;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public class UserController : ControllerBase
{
    readonly ILogger<UserController>? _logger;
    readonly UserService _userService;

    public UserController(UserService userService, ILogger<UserController>? logger = null)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<string>> AddUser([FromBody] AddUserDto addUserDto)
    {
        try
        {
            await _userService.AddUserAsync(
                new UserRootEntity(new EmailAddress(addUserDto.Email), addUserDto.FirstName, addUserDto.LastName,
                    addUserDto.TenantId),
                CancellationToken.None);

            return Ok("User added successfully");
        }
        catch (Exception e) when (e.Message.Contains("duplicate key value violates unique constraint"))
        {
            return Conflict("User already exists");
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Error adding user");
            return StatusCode(500, "Internal server error");
        }
    }
    
    [HttpDelete("{email}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<string>> RemoveUser([FromRoute] string email)
    {
        try
        {
            await _userService.RemoveUserAsync(email, CancellationToken.None);
            return Ok("User removed successfully");
        }
        catch (KeyNotFoundException)
        {
            return NotFound("User not found");
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Error removing user");
            return StatusCode(500, "Internal server error");
        }
    }
}

public record AddUserDto(
    [Required] string FirstName,
    [Required] string LastName,
    [Required] string Email,
    [Required] string TenantId);
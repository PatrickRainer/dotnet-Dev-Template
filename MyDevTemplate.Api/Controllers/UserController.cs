using Microsoft.AspNetCore.Mvc;
using MyDevTemplate.Application.UserServices;
using MyDevTemplate.Domain.Entities.Common;
using MyDevTemplate.Domain.Entities.UserAggregate;

namespace MyDevTemplate.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
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
    public async Task<ActionResult<string>> AddUser([FromBody] UserDto userDto)
    {
        try
        {
            await _userService.AddUserAsync(
                new UserRootEntity(new EmailAddress(userDto.Email), userDto.FirstName, userDto.LastName,
                    userDto.TenantId),
                CancellationToken.None);

            return Ok("User added successfully");
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Error adding user");
            return StatusCode(500, "Internal server error");
        }
    }
}

public record UserDto(string FirstName, string LastName, string Email, string TenantId);
using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyDevTemplate.Application.UserServices;
using MyDevTemplate.Application.UserServices.Dtos;
using MyDevTemplate.Domain.Entities.Common;
using MyDevTemplate.Domain.Entities.UserAggregate;

namespace MyDevTemplate.Api.Controllers;

[Authorize]
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController>? _logger;
    private readonly IUserService _userService;
    private readonly IValidator<AddUserDto> _addValidator;
    private readonly IValidator<UpdateUserDto> _updateValidator;

    public UserController(
        IUserService userService, 
        IValidator<AddUserDto> addValidator,
        IValidator<UpdateUserDto> updateValidator,
        ILogger<UserController>? logger = null)
    {
        _userService = userService;
        _addValidator = addValidator;
        _updateValidator = updateValidator;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<UserRoot>))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<UserRoot>>> GetAllUsers(CancellationToken cancellationToken)
    {
        try
        {
            var users = await _userService.GetAllAsync(cancellationToken);
            return Ok(users);
        }
        catch (OperationCanceledException)
        {
            return StatusCode(499);
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Error getting all users");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("email/{email}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserRoot))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserRoot>> GetUserByEmail([FromRoute] string email, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userService.GetUserByEmailAsync(email, cancellationToken);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }
        catch (OperationCanceledException)
        {
            return StatusCode(499);
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Error getting user by email");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserRoot))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserRoot>> GetUser(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userService.GetByIdAsync(id, cancellationToken);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }
        catch (OperationCanceledException)
        {
            return StatusCode(499);
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Error getting user with id {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<string>> AddUser([FromBody] AddUserDto addUserDto, CancellationToken cancellationToken)
    {
        try
        {
            await _addValidator.ValidateAndThrowAsync(addUserDto, cancellationToken);
            
            var user = new UserRoot(
                new EmailAddress(addUserDto.Email),
                addUserDto.FirstName,
                addUserDto.LastName,
                addUserDto.IdentityProviderId);

            await _userService.AddAsync(user, cancellationToken);

            return Ok("User added successfully");
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

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<string>> UpdateUser(Guid id, [FromBody] UpdateUserDto updateUserDto, CancellationToken cancellationToken)
    {
        try
        {
            await _updateValidator.ValidateAndThrowAsync(updateUserDto, cancellationToken);
            
            var user = await _userService.GetByIdAsync(id, cancellationToken);
            if (user == null)
            {
                return NotFound("User not found");
            }

            user.FirstName = updateUserDto.FirstName;
            user.LastName = updateUserDto.LastName;

            await _userService.UpdateAsync(user, cancellationToken);

            return Ok("User updated successfully");
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
            _logger?.LogError(e, "Error updating user with id {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }
    
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<string>> RemoveUser(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _userService.DeleteAsync(id, cancellationToken);
            return Ok("User removed successfully");
        }
        catch (OperationCanceledException)
        {
            return StatusCode(499);
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

    [HttpDelete("email/{email}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<string>> RemoveUserByEmail([FromRoute] string email, CancellationToken cancellationToken)
    {
        try
        {
            await _userService.RemoveUserByEmailAsync(email, cancellationToken);
            return Ok("User removed successfully");
        }
        catch (OperationCanceledException)
        {
            return StatusCode(499);
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
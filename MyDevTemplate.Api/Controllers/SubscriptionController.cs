using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyDevTemplate.Application.SubscriptionServices;
using MyDevTemplate.Application.SubscriptionServices.Dtos;
using MyDevTemplate.Domain.Entities.SubscriptionAggregate;

namespace MyDevTemplate.Api.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[Authorize(Policy = "MasterTenant")]
public class SubscriptionController : ControllerBase
{
    private readonly SubscriptionService _subscriptionService;
    private readonly IValidator<CreateSubscriptionDto> _createValidator;
    private readonly IValidator<UpdateSubscriptionDto> _updateValidator;
    private readonly ILogger<SubscriptionController>? _logger;

    public SubscriptionController(
        SubscriptionService subscriptionService,
        IValidator<CreateSubscriptionDto> createValidator,
        IValidator<UpdateSubscriptionDto> updateValidator,
        ILogger<SubscriptionController>? logger = null)
    {
        _subscriptionService = subscriptionService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous] // Allow anyone to see available subscriptions (e.g., for registration)
    public async Task<ActionResult<IEnumerable<SubscriptionRoot>>> GetAllSubscriptions(CancellationToken cancellationToken)
    {
        try
        {
            var subscriptions = await _subscriptionService.GetAllAsync(cancellationToken);
            return Ok(subscriptions);
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Error getting all subscriptions");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<SubscriptionRoot>> GetSubscription(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var subscription = await _subscriptionService.GetByIdAsync(id, cancellationToken);
            if (subscription == null) return NotFound();
            return Ok(subscription);
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Error getting subscription {SubscriptionId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost]
    public async Task<ActionResult<SubscriptionRoot>> CreateSubscription([FromBody] CreateSubscriptionDto dto, CancellationToken cancellationToken)
    {
        var validationResult = await _createValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid) return BadRequest(validationResult.Errors);

        try
        {
            var subscription = new SubscriptionRoot(dto.Name, dto.Description)
            {
                Features = dto.Features
            };

            var id = await _subscriptionService.AddAsync(subscription, cancellationToken);
            return CreatedAtAction(nameof(GetSubscription), new { id, version = "1.0" }, subscription);
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Error creating subscription {SubscriptionName}", dto.Name);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSubscription(Guid id, [FromBody] UpdateSubscriptionDto dto, CancellationToken cancellationToken)
    {
        var validationResult = await _updateValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid) return BadRequest(validationResult.Errors);

        try
        {
            var subscription = await _subscriptionService.GetByIdAsync(id, cancellationToken);
            if (subscription == null) return NotFound();

            subscription.Name = dto.Name;
            subscription.Description = dto.Description;
            subscription.Features = dto.Features;

            await _subscriptionService.UpdateAsync(subscription, cancellationToken);
            return NoContent();
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Error updating subscription {SubscriptionId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSubscription(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _subscriptionService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Error deleting subscription {SubscriptionId}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}

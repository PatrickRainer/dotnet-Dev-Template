using FluentValidation;
using Microsoft.Extensions.Logging;
using MyDevTemplate.Application.RegistrationServices.Dtos;
using MyDevTemplate.Domain.Entities.TenantAggregate;
using MyDevTemplate.Persistence;

namespace MyDevTemplate.Application.RegistrationServices;

public class RegistrationService : IRegistrationService
{
    private readonly AppDbContext _dbContext;
    private readonly IValidator<TenantRoot> _tenantValidator;
    private readonly ILogger<RegistrationService> _logger;

    public RegistrationService(
        AppDbContext dbContext, 
        IValidator<TenantRoot> tenantValidator, 
        ILogger<RegistrationService> logger)
    {
        _dbContext = dbContext;
        _tenantValidator = tenantValidator;
        _logger = logger;
    }

    public async Task<Guid> RegisterAsync(RegistrationDto registrationDto, string adminEmail, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Registering new company: {CompanyName} for admin: {AdminEmail}", 
                registrationDto.CompanyName, adminEmail);

            var newTenant = new TenantRoot(
                registrationDto.CompanyName, 
                registrationDto.CompanyName,
                adminEmail, 
                registrationDto.SubscriptionId);

            newTenant.AddAddress(
                registrationDto.Street, 
                registrationDto.City, 
                string.Empty,
                registrationDto.Country, 
                registrationDto.ZipCode);

            // Validate the tenant entity
            await _tenantValidator.ValidateAndThrowAsync(newTenant, cancellationToken);

            // Add to database
            // Note: Since this is a new tenant registration, we might need to ensure the 
            // SaveChangesAsync doesn't fail due to tenant isolation if the current provider returns null.
            // However, TenantRoot is excluded from the global query filter and isolation in AppDbContext.
            
            await _dbContext.Tenants.AddAsync(newTenant, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Company registered successfully. TenantId: {TenantId}", newTenant.Id);

            return newTenant.Id;
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation failed during registration for {CompanyName}", registrationDto.CompanyName);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during registration for {CompanyName}", registrationDto.CompanyName);
            throw;
        }
    }
}

using FluentValidation;
using MyDevTemplate.Application.Common.Validations;
using MyDevTemplate.Application.TenantServices;

namespace MyDevTemplate.Blazor.Server.Components.Pages.Company.TenantManagement;

public class TenantManagementValidator : AbstractValidator<TenantManagementModel>
{
    public TenantManagementValidator()
    {
        RuleFor(x => x.TenantName)
            .NotEmpty().WithMessage("Tenant Name is required")
            .MaximumLength(100).WithMessage("Tenant Name must not exceed 100 characters");

        RuleFor(x => x.CompanyName).ApplyCompanyNameRules();

        RuleFor(x => x.AdminEmail)
            .NotEmpty().WithMessage("Admin Email is required")
            .EmailAddress().WithMessage("A valid email address is required");

        RuleFor(x => x.Street).ApplyStreetRules();
        RuleFor(x => x.City).ApplyCityRules();
        RuleFor(x => x.ZipCode).ApplyZipCodeRules();
        RuleFor(x => x.Country).ApplyCountryRules();
    }

    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<TenantManagementModel>.CreateWithOptions((TenantManagementModel)model, x => x.IncludeProperties(propertyName)));
        if (result.IsValid)
            return Array.Empty<string>();
        return result.Errors.Select(e => e.ErrorMessage);
    };
}

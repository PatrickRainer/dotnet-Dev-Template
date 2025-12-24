using FluentValidation;
using MyDevTemplate.Application.Common.Validations;
using MyDevTemplate.Application.TenantServices;
using MyDevTemplate.Blazor.Server.Models;

namespace MyDevTemplate.Blazor.Server.Validators;

public class CompanyRegistrationPageModelValidator : AbstractValidator<CompanyRegistrationPageModel>
{
    public CompanyRegistrationPageModelValidator()
    {
        RuleFor(x => x.CompanyName).ApplyCompanyNameRules();
        RuleFor(x => x.Street).ApplyStreetRules();
        RuleFor(x => x.City).ApplyCityRules();
        RuleFor(x => x.ZipCode).ApplyZipCodeRules();
        RuleFor(x => x.Country).ApplyCountryRules();
    }

    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result =
            await ValidateAsync(ValidationContext<CompanyRegistrationPageModel>.CreateWithOptions(
                (CompanyRegistrationPageModel)model,
                x => x.IncludeProperties(propertyName)));
        if (result.IsValid)
            return Array.Empty<string>();
        return result.Errors.Select(e => e.ErrorMessage);
    };
}

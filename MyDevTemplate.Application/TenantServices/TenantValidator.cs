using FluentValidation;
using MyDevTemplate.Application.Common.Validations;
using MyDevTemplate.Domain.Entities.TenantAggregate;

namespace MyDevTemplate.Application.TenantServices;

public class TenantValidator : AbstractValidator<TenantRoot>
{
    public TenantValidator()
    {
        RuleFor(x => x.TenantName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.CompanyName).ApplyCompanyNameRules();
        RuleFor(x => x.Address).SetValidator(new AddressValidator());
        RuleFor(x => x.AdminEmail).NotEmpty().MaximumLength(256).EmailAddress();
    }

    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result =
            await ValidateAsync(ValidationContext<TenantRoot>.CreateWithOptions((TenantRoot)model,
                x => x.IncludeProperties(propertyName)));
        if (result.IsValid)
            return Array.Empty<string>();
        return result.Errors.Select(e => e.ErrorMessage);
    };
}

public static class TenantValidationRules
{
    public static IRuleBuilderOptions<T, string> ApplyCompanyNameRules<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .MaximumLength(200);
    }
}
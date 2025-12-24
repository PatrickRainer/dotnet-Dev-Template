using FluentValidation;
using MyDevTemplate.Domain.Entities.TenantAggregate;

namespace MyDevTemplate.Application.TenantServices;

public class TenantValidator : AbstractValidator<TenantRoot>
{
    public TenantValidator()
    {
        RuleFor(x => x.TenantName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.CompanyName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Address.Street).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Address.City).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Address.State).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Address.Country).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Address.ZipCode).NotEmpty().MaximumLength(20);
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
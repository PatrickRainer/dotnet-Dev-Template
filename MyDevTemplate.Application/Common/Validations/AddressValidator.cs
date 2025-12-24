using FluentValidation;
using MyDevTemplate.Domain.Entities.Common;

namespace MyDevTemplate.Application.Common.Validations;

public class AddressValidator : AbstractValidator<Address>
{
    public AddressValidator()
    {
        RuleFor(x => x.Street).ApplyStreetRules();
        RuleFor(x => x.City).ApplyCityRules();
        RuleFor(x => x.State).ApplyStateRules();
        RuleFor(x => x.Country).ApplyCountryRules();
        RuleFor(x => x.ZipCode).ApplyZipCodeRules();
    }
}

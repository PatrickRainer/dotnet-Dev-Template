using FluentValidation;
using MyDevTemplate.Application.Common.Validations;
using MyDevTemplate.Application.TenantServices.Dtos;

namespace MyDevTemplate.Application.TenantServices;

public class CreateTenantDtoValidator : AbstractValidator<CreateTenantDto>
{
    public CreateTenantDtoValidator()
    {
        RuleFor(x => x.TenantName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.CompanyName).ApplyCompanyNameRules();
        RuleFor(x => x.AdminEmail).NotEmpty().MaximumLength(256).EmailAddress();
        RuleFor(x => x.Street).ApplyStreetRules().When(x => x.Street != null);
        RuleFor(x => x.City).ApplyCityRules().When(x => x.City != null);
        RuleFor(x => x.State).ApplyStateRules().When(x => x.State != null);
        RuleFor(x => x.Country).ApplyCountryRules().When(x => x.Country != null);
        RuleFor(x => x.ZipCode).ApplyZipCodeRules().When(x => x.ZipCode != null);
    }
}

public class UpdateTenantDtoValidator : AbstractValidator<UpdateTenantDto>
{
    public UpdateTenantDtoValidator()
    {
        RuleFor(x => x.TenantName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.CompanyName).ApplyCompanyNameRules();
        RuleFor(x => x.Street).ApplyStreetRules().When(x => x.Street != null);
        RuleFor(x => x.City).ApplyCityRules().When(x => x.City != null);
        RuleFor(x => x.State).ApplyStateRules().When(x => x.State != null);
        RuleFor(x => x.Country).ApplyCountryRules().When(x => x.Country != null);
        RuleFor(x => x.ZipCode).ApplyZipCodeRules().When(x => x.ZipCode != null);
    }
}

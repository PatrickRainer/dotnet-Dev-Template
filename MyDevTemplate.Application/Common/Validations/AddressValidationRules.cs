using FluentValidation;

namespace MyDevTemplate.Application.Common.Validations;

public static class AddressValidationRules
{
    public static IRuleBuilderOptions<T, string?> ApplyStreetRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .MaximumLength(200);
    }
    
    public static IRuleBuilderOptions<T, string?> ApplyCityRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .MaximumLength(100);
    }

    public static IRuleBuilderOptions<T, string?> ApplyStateRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(100);
    }

    public static IRuleBuilderOptions<T, string?> ApplyCountryRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .MaximumLength(100);
    }

    public static IRuleBuilderOptions<T, string?> ApplyZipCodeRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .MaximumLength(20);
    }
}
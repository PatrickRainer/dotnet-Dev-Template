using FluentValidation;

namespace MyDevTemplate.Application.Common.Validations;

public static class EmailValidationRules
{
    public static IRuleBuilderOptions<T, string> ApplyEmailRules<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .MaximumLength(256)
            .EmailAddress();
    }
}

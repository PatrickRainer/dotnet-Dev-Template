using FluentValidation;
using MyDevTemplate.Domain.Entities.ApiKeyAggregate;

namespace MyDevTemplate.Application.ApiKeyServices;

public class ApiKeyValidator : AbstractValidator<ApiKeyRoot>
{
    public ApiKeyValidator()
    {
        RuleFor(x => x.Key)
            .NotEmpty()
            .MaximumLength(256);

        RuleFor(x => x.Label)
            .NotEmpty()
            .MaximumLength(100);
    }
}

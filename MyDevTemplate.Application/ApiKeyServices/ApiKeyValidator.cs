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

        // All API keys must belong to a tenant to ensure data isolation.
        // NOTE: If migrating legacy data with empty TenantIds, ensure they are 
        // assigned a valid TenantId before this validation is enforced during updates.
        RuleFor(x => x.TenantId)
            .NotEmpty();
    }
}

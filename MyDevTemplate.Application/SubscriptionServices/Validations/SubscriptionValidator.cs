using FluentValidation;
using MyDevTemplate.Domain.Entities.SubscriptionAggregate;

namespace MyDevTemplate.Application.SubscriptionServices.Validations;

public class SubscriptionValidator : AbstractValidator<SubscriptionRoot>
{
    public SubscriptionValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Description).MaximumLength(1000);
    }
}

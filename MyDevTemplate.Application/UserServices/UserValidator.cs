using FluentValidation;
using MyDevTemplate.Application.Common.Validations;
using MyDevTemplate.Domain.Entities.UserAggregate;

namespace MyDevTemplate.Application.UserServices;

public class UserValidator : AbstractValidator<UserRoot>
{
    public UserValidator()
    {
        RuleFor(x => x.Email).SetValidator(new EmailAddressValidator());

        RuleFor(x => x.FirstName)
            .NotNull()
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotNull()
            .MaximumLength(100);

        RuleFor(x => x.IdentityProviderId)
            .NotEmpty()
            .MaximumLength(100);
    }
}

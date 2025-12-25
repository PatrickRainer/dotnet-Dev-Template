using FluentValidation;
using MyDevTemplate.Domain.Entities.UserAggregate;

namespace MyDevTemplate.Application.UserGroupServices;

public class UserGroupValidator : AbstractValidator<UserGroup>
{
    public UserGroupValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Description)
            .MaximumLength(500);
    }
}

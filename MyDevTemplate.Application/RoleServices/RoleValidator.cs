using FluentValidation;
using MyDevTemplate.Domain.Entities.RoleAggregate;

namespace MyDevTemplate.Application.RoleServices;

public class RoleValidator : AbstractValidator<RoleRoot>
{
    public RoleValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(256);

        RuleFor(x => x.Description)
            .MaximumLength(1000);
    }
}

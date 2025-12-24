using FluentValidation;

namespace MyDevTemplate.Application.RoleServices.Dtos;

public class AddRoleDtoValidator : AbstractValidator<AddRoleDto>
{
    public AddRoleDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(256);

        RuleFor(x => x.Description)
            .MaximumLength(1000);
    }
}

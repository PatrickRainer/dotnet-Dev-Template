using FluentValidation;
using MyDevTemplate.Application.Common.Validations;

namespace MyDevTemplate.Application.UserServices.Dtos;

public class AddUserDtoValidator : AbstractValidator<AddUserDto>
{
    public AddUserDtoValidator()
    {
        RuleFor(x => x.Email).ApplyEmailRules();

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.IdentityProviderId)
            .NotEmpty()
            .MaximumLength(100);
    }
}

using FluentValidation;

namespace MyDevTemplate.Blazor.Server.Components.Pages.Company.UserManagement;

public class UserManagementValidator : AbstractValidator<UserManagementModel>
{
    public UserManagementValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("A valid email address is required");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First Name is required")
            .MaximumLength(100).WithMessage("First Name must not exceed 100 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last Name is required")
            .MaximumLength(100).WithMessage("Last Name must not exceed 100 characters");

        RuleFor(x => x.IdentityProviderId)
            .MaximumLength(100).WithMessage("Identity Provider Id must not exceed 100 characters");
    }

    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<UserManagementModel>.CreateWithOptions((UserManagementModel)model, x => x.IncludeProperties(propertyName)));
        if (result.IsValid)
            return Array.Empty<string>();
        return result.Errors.Select(e => e.ErrorMessage);
    };
}

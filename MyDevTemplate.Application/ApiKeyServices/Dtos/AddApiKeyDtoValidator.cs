using FluentValidation;

namespace MyDevTemplate.Application.ApiKeyServices.Dtos;

public class AddApiKeyDtoValidator : AbstractValidator<AddApiKeyDto>
{
    public AddApiKeyDtoValidator()
    {
        RuleFor(x => x.Key)
            .NotEmpty()
            .MaximumLength(256);

        RuleFor(x => x.Label)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.TenantId)
            .Must(x => string.IsNullOrEmpty(x) || Guid.TryParse(x, out _))
            .WithMessage("TenantId must be a valid GUID");
    }
}

using FluentValidation;

namespace MyDevTemplate.Application.ApiKeyServices.Dtos;

public class UpdateApiKeyDtoValidator : AbstractValidator<UpdateApiKeyDto>
{
    public UpdateApiKeyDtoValidator()
    {
        RuleFor(x => x.Label)
            .NotEmpty()
            .MaximumLength(100);
    }
}

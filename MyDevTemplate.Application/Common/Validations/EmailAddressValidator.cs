using FluentValidation;
using MyDevTemplate.Domain.Entities.Common;

namespace MyDevTemplate.Application.Common.Validations;

public class EmailAddressValidator : AbstractValidator<EmailAddress>
{
    public EmailAddressValidator()
    {
        RuleFor(x => x.Value).ApplyEmailRules();
    }
}

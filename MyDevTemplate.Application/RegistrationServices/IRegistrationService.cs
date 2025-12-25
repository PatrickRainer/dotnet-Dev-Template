using MyDevTemplate.Application.RegistrationServices.Dtos;

namespace MyDevTemplate.Application.RegistrationServices;

public interface IRegistrationService
{
    Task<Guid> RegisterAsync(RegistrationDto registrationDto, string adminEmail, CancellationToken cancellationToken = default);
}

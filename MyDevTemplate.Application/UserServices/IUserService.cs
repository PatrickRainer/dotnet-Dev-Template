using MyDevTemplate.Application.Common.Interfaces;
using MyDevTemplate.Domain.Entities.UserAggregate;

namespace MyDevTemplate.Application.UserServices;

public interface IUserService : ICrudService<UserRoot, Guid>
{
    Task<UserRoot?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<int> RemoveUserByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<Guid> UpsertAfterLogin(string identityProviderId, string email);
    Task AddFeatureToUserAsync(Guid userId, string feature, CancellationToken cancellationToken = default);
    Task RemoveFeatureFromUserAsync(Guid userId, string feature, CancellationToken cancellationToken = default);
    Task AddUserToGroupAsync(Guid userId, Guid groupId, CancellationToken cancellationToken = default);
    Task RemoveUserFromGroupAsync(Guid userId, Guid groupId, CancellationToken cancellationToken = default);
}

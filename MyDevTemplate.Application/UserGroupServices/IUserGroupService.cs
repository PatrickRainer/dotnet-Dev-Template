using MyDevTemplate.Application.Common.Interfaces;
using MyDevTemplate.Domain.Entities.UserAggregate;

namespace MyDevTemplate.Application.UserGroupServices;

public interface IUserGroupService : ICrudService<UserGroup, Guid>
{
    Task AddFeatureToGroupAsync(Guid groupId, string feature, CancellationToken cancellationToken = default);
    Task RemoveFeatureFromGroupAsync(Guid groupId, string feature, CancellationToken cancellationToken = default);
    Task AddUserToGroupAsync(Guid groupId, Guid userId, CancellationToken cancellationToken = default);
    Task RemoveUserFromGroupAsync(Guid groupId, Guid userId, CancellationToken cancellationToken = default);
}

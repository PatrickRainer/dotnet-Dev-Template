using MyDevTemplate.Application.Common.Interfaces;
using MyDevTemplate.Domain.Entities.RoleAggregate;

namespace MyDevTemplate.Application.RoleServices;

public interface IRoleService : ICrudService<RoleRoot, Guid>
{
}

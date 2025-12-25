using MyDevTemplate.Application.Common.Interfaces;
using MyDevTemplate.Domain.Entities.TenantAggregate;

namespace MyDevTemplate.Application.TenantServices;

public interface ITenantService : ICrudService<TenantRoot, Guid>
{
}

using MyDevTemplate.Application.Common.Interfaces;
using MyDevTemplate.Domain.Entities.SubscriptionAggregate;

namespace MyDevTemplate.Application.SubscriptionServices;

public interface ISubscriptionService : ICrudService<SubscriptionRoot, Guid>
{
}

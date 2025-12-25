using MyDevTemplate.Application.Common.Interfaces;
using MyDevTemplate.Domain.Entities.ApiKeyAggregate;

namespace MyDevTemplate.Application.ApiKeyServices;

public interface IApiKeyService : ICrudService<ApiKeyRoot, Guid>
{
    Task<ApiKeyRoot?> ValidateApiKeyAsync(Guid tenantId, string key, CancellationToken cancellationToken = default);
}

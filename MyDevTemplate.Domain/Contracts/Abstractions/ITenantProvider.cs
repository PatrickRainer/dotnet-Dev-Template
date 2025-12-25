namespace MyDevTemplate.Domain.Contracts.Abstractions;

public interface ITenantProvider
{
    Guid? GetTenantId();
    bool IsMasterTenant();
    Guid? GetMasterTenantId();
}

using MyDevTemplate.Domain.Entities.Abstractions;
using MyDevTemplate.Domain.Entities.Common;

namespace MyDevTemplate.Domain.Entities.TenantAggregate;

public class TenantRoot : EntityBase
{
    // For EF Core
    TenantRoot()
    {
        TenantName = string.Empty;
        CompanyName = string.Empty;
    }
    
    public TenantRoot(string tenantName, string companyName)
    {
        TenantName = tenantName;
        CompanyName = companyName;
    }

    public string TenantName { get; set; }
    public string CompanyName { get; set; }
    public Address Address { get; set; } = new(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
    
    public override string ToString()
    {
        return $"TenantId: {Id}, Tenant Name: {TenantName}";
    }
}


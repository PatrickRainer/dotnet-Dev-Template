using MyDevTemplate.Domain.Entities.Abstractions;
using MyDevTemplate.Domain.Entities.Common;

namespace MyDevTemplate.Domain.Entities.TenantAggregate;

public class TenantRoot : EntityBase
{
    // For EF Core
    TenantRoot()
    {
        AdminEmail =string.Empty;
        TenantName = string.Empty;
        CompanyName = string.Empty;
    }
    
    public TenantRoot(string tenantName, string companyName, string adminEmail)
    {
        TenantName = tenantName;
        CompanyName = companyName;
        AdminEmail = adminEmail;
    }

    public string TenantName { get; set; }
    public string CompanyName { get; set; }
    public Address Address { get; private set; } = new(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
    public string AdminEmail { get; set; }

    public override string ToString()
    {
        return $"TenantId: {Id}, Tenant Name: {TenantName}";
    }

    public void AddAddress(string street, string city, string zipCode, string country, string state)
    {
        Address = new Address(street, city, state,zipCode, country);
    }
}


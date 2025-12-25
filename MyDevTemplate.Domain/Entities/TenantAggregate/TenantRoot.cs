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
    
    public TenantRoot(string tenantName, string companyName, string adminEmail, Guid? subscriptionId = null)
    {
        TenantName = tenantName;
        CompanyName = companyName;
        AdminEmail = adminEmail;
        SubscriptionId = subscriptionId;
        
        // For TenantRoot, the TenantId (from EntityBase) must always be its own Id.
        TenantId = Id;
    }

    public string TenantName { get; set; }
    public string CompanyName { get; set; }
    public Guid? SubscriptionId { get; set; }
    public Address Address { get; private set; } = new(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
    public string AdminEmail { get; set; }
    
    /// <summary>
    /// Returns the Tenant identifier. 
    /// For TenantRoot, this is always the same as the Id inherited from EntityBase.
    /// </summary>
    public new Guid TenantId 
    { 
        get => Id; 
        init => base.TenantId = value; 
    }

    public override string ToString()
    {
        return $"TenantId: {Id}, Tenant Name: {TenantName}";
    }

    public void AddAddress(string street, string city, string state, string country, string zipCode)
    {
        Address = new Address(street, city, state, country, zipCode);
    }
}


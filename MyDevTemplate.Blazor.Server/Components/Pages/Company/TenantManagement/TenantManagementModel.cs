namespace MyDevTemplate.Blazor.Server.Components.Pages.Company.TenantManagement;

public class TenantManagementModel
{
    public Guid Id { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string AdminEmail { get; set; } = string.Empty;
    public Guid? SubscriptionId { get; set; }
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}

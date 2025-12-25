namespace MyDevTemplate.Blazor.Server.Components.Pages.Company.UserManagement;

public class UserManagementModel
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string IdentityProviderId { get; set; } = string.Empty;
}

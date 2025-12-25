namespace MyDevTemplate.Blazor.Server.Components.Pages.Company.SubscriptionManagement;

public class SubscriptionModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> SelectedFeatures { get; set; } = new();
}

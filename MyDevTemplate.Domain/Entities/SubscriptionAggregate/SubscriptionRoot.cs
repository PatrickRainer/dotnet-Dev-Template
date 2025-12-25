using MyDevTemplate.Domain.Entities.Abstractions;

namespace MyDevTemplate.Domain.Entities.SubscriptionAggregate;

public class SubscriptionRoot : EntityBase
{
    // For EF Core
    private SubscriptionRoot()
    {
        Name = string.Empty;
        Description = string.Empty;
    }

    public SubscriptionRoot(string name, string description = "")
    {
        Name = name;
        Description = description;
    }

    public string Name { get; set; }
    public string Description { get; set; }
    public List<string> Features { get; set; } = new();
}

public static class SubscriptionFeatures
{
    public const string Dashboard = "Dashboard";
    public const string Analytics = "Analytics";
    public const string Settings = "Settings";
    public const string Reports = "Reports";
    public const string Notifications = "Notifications";
    public const string UserManagement = "UserManagement";
    public const string Integration = "Integration";
    public const string AdvancedAnalytics = "AdvancedAnalytics";
    public const string Automation = "Automation";
    public const string Security = "Security";
    public const string ApiAccess = "ApiAccess";
    public const string CustomWorkflows = "CustomWorkflows";
}

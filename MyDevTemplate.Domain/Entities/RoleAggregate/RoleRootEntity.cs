using MyDevTemplate.Domain.Entities.Abstractions;

namespace MyDevTemplate.Domain.Entities.RoleAggregate;

public class RoleRootEntity : EntityBase
{
    // For Ef Core
    RoleRootEntity()
    {
        Title = string.Empty;
    }

    public RoleRootEntity(string title, string description = "")
    {
        Title = title;
    }

    public string Title { get; set; }
    public string Description { get; set; } = string.Empty;
}
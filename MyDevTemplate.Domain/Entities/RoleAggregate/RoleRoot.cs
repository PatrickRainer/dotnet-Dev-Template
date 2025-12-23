using MyDevTemplate.Domain.Entities.Abstractions;

namespace MyDevTemplate.Domain.Entities.RoleAggregate;

public class RoleRoot : EntityBase
{
    // For Ef Core
    RoleRoot()
    {
        Title = string.Empty;
    }

    public RoleRoot(string title, string description = "")
    {
        Title = title;
        Description = description;
    }

    public string Title { get; set; }
    public string Description { get; set; } = string.Empty;
    
    // Navigation Properties
    private readonly List<Guid> _users = new();
    public IReadOnlyCollection<Guid> Users => _users.AsReadOnly();
    
    public void AddUser(Guid userId)
    {
        if (!_users.Contains(userId))
        {
            _users.Add(userId);
        }
    }
    
    public void RemoveUser(Guid userId)
    {
        _users.Remove(userId);
    }
}
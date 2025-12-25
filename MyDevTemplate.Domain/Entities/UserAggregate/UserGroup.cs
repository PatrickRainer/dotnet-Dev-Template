using MyDevTemplate.Domain.Entities.Abstractions;

namespace MyDevTemplate.Domain.Entities.UserAggregate;

public class UserGroup : EntityBase
{
    // For Ef Core
    private UserGroup()
    {
        Name = string.Empty;
        Description = string.Empty;
    }

    public UserGroup(string name, string description = "")
    {
        Name = name;
        Description = description;
    }

    public string Name { get; set; }
    public string Description { get; set; }

    // Navigation Properties
    readonly List<UserRoot> _users = new();
    public IReadOnlyCollection<UserRoot> Users => _users.AsReadOnly();
    readonly List<string> _features = new();
    public IReadOnlyCollection<string> AllowedFeatures => _features.AsReadOnly();
    
    // Domain Methods
    public void AddUser(UserRoot user)
    {
        if (!_users.Contains(user))
        {
            _users.Add(user);
        }
    }
    
    public void RemoveUser(UserRoot user)
    {
        _users.Remove(user);
    }
    
    public void AddFeature(string feature)
    {
        if (!_features.Contains(feature))
        {
            _features.Add(feature);
        }
    }
    
    public void RemoveFeature(string feature)
    {
        _features.Remove(feature);
    }
}
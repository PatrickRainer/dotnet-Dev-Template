using MyDevTemplate.Domain.Entities.Abstractions;
using MyDevTemplate.Domain.Entities.Common;

namespace MyDevTemplate.Domain.Entities.UserAggregate;

public class UserRoot : EntityBase
{
    // For Ef Core
    private UserRoot()
    {
        LastName = string.Empty;
        FirstName = string.Empty;
        IdentityProviderId = string.Empty;
    }

    public UserRoot(EmailAddress email, string firstName, string lastName, string identityProviderId)
    {
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        IdentityProviderId = identityProviderId;
    }

    public EmailAddress Email { get; set; } = null!;
    public string Username => Email.Value;
    public string LastName { get; set; }
    public string FirstName { get; set; }
    public string FullName => $"{FirstName} {LastName}";
    public string IdentityProviderId { get; set; }
    public DateTime? LastLoginAtUtc { get; set; }

    public bool IsAssignedToTenant => TenantId != Guid.Empty;

    // Navigation Properties
    readonly List<Guid> _roles = new();
    public IReadOnlyCollection<Guid> Roles => _roles.AsReadOnly();
    
    readonly List<string> _features = new();
    public IReadOnlyCollection<string> AllowedFeatures => _features.AsReadOnly();
    readonly List<UserGroup> _groups = new();
    public IReadOnlyCollection<UserGroup> Groups => _groups.AsReadOnly();
    
    
    // Domain Methods
    public void AddRole(Guid roleId)
    {
        if (!_roles.Contains(roleId))
        {
            _roles.Add(roleId);
        }
    }
    
    public void RemoveRole(Guid roleId)
    {
        _roles.Remove(roleId);
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
    
    public void AddToGroup(UserGroup group)
    {
        if (!_groups.Contains(group))
        {
            _groups.Add(group);
        }
    }
    
    public void RemoveFromGroup(UserGroup group)
    {
        _groups.Remove(group);
    }

}
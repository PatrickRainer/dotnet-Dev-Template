using MyDevTemplate.Domain.Entities.Abstractions;
using MyDevTemplate.Domain.Entities.Common;

namespace MyDevTemplate.Domain.Entities.UserAggregate;

public class UserRootEntity : EntityBase
{
    // For Ef Core
    private UserRootEntity()
    {
        LastName = string.Empty;
        FirstName = string.Empty;
        IdentityProviderId = string.Empty;
    }

    public UserRootEntity(EmailAddress email, string firstName, string lastName, string identityProviderId)
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

    // Navigation Properties
    readonly List<Guid> _roles = new List<Guid>();
    public IReadOnlyCollection<Guid> Roles => _roles.AsReadOnly();
    
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

}
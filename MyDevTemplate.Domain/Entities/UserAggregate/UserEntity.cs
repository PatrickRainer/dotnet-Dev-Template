using MyDevTemplate.Domain.Entities.Abstractions;
using MyDevTemplate.Domain.Entities.Common;

namespace MyDevTemplate.Domain.Entities.UserAggregate;

public class UserRootEntity : EntityBase
{
    // For Ef Core
    UserRootEntity()
    {
        LastName = string.Empty;
        FirstName = string.Empty;
    }

    public EmailAddress Email { get; set; } = null!;
    public string Username => Email.Value;
    public string LastName { get; set; }
    public string FirstName { get; set; }
    public string FullName => $"{FirstName} {LastName}";


}
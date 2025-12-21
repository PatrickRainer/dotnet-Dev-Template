using MyDevTemplate.Domain.Entities.Common;
using MyDevTemplate.Domain.Entities.UserAggregate;
using Xunit;

namespace MyDevTemplate.Domain.Tests;

public class UserRootEntityTests
{
    [Fact]
    public void UserRootEntity_Should_Initialize_Correctly()
    {
        // Arrange
        var emailStr = "test@example.com";
        var firstName = "John";
        var lastName = "Doe";
        var email = new EmailAddress(emailStr);
        var identiyProviderId = Guid.NewGuid().ToString();

        // Act
        var user = new UserRootEntity(email, firstName, lastName, identiyProviderId);

        // Assert
        Assert.Equal(email, user.Email);
        Assert.Equal(firstName, user.FirstName);
        Assert.Equal(lastName, user.LastName);
        Assert.Equal(emailStr, user.Username);
        Assert.Equal($"{firstName} {lastName}", user.FullName);
        Assert.Empty(user.Roles);
        Assert.NotEqual(Guid.Empty, user.Id);
    }

    [Fact]
    public void AddRole_Should_Add_Role_To_Collection()
    {
        // Arrange
        var user = new UserRootEntity(new EmailAddress("test@example.com"), "John", "Doe", Guid.NewGuid().ToString());
        var roleId = Guid.NewGuid();

        // Act
        user.AddRole(roleId);

        // Assert
        Assert.Contains(roleId, user.Roles);
    }

    [Fact]
    public void RemoveRole_Should_Remove_Role_From_Collection()
    {
        // Arrange
        var user = new UserRootEntity(new EmailAddress("test@example.com"), "John", "Doe", Guid.NewGuid().ToString());
        var roleId = Guid.NewGuid();
        user.AddRole(roleId);

        // Act
        user.RemoveRole(roleId);

        // Assert
        Assert.DoesNotContain(roleId, user.Roles);
    }
}

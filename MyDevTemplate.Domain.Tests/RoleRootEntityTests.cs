using MyDevTemplate.Domain.Entities.RoleAggregate;
using Xunit;

namespace MyDevTemplate.Domain.Tests;

public class RoleRootEntityTests
{
    [Fact]
    public void RoleRootEntity_Should_Initialize_Correctly()
    {
        // Arrange
        var title = "Admin";
        var description = "Administrator role";

        // Act
        var role = new RoleRootEntity(title, description);

        // Assert
        Assert.Equal(title, role.Title);
        Assert.Equal(description, role.Description);
        Assert.Empty(role.Users);
        Assert.NotEqual(Guid.Empty, role.Id);
    }

    [Fact]
    public void AddUser_Should_Add_User_To_Collection()
    {
        // Arrange
        var role = new RoleRootEntity("Admin");
        var userId = Guid.NewGuid();

        // Act
        role.AddUser(userId);

        // Assert
        Assert.Contains(userId, role.Users);
    }

    [Fact]
    public void RemoveUser_Should_Remove_User_From_Collection()
    {
        // Arrange
        var role = new RoleRootEntity("Admin");
        var userId = Guid.NewGuid();
        role.AddUser(userId);

        // Act
        role.RemoveUser(userId);

        // Assert
        Assert.DoesNotContain(userId, role.Users);
    }
}

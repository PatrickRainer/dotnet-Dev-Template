using MyDevTemplate.Domain.Entities.Common;
using MyDevTemplate.Domain.Entities.UserAggregate;

namespace MyDevTemplate.Domain.Tests;

public class UserGroupTests
{
    [Fact]
    public void UserGroup_Should_Initialize_Correctly()
    {
        // Arrange
        var name = "Admins";
        var description = "Administrator group";

        // Act
        var group = new UserGroup(name, description);

        // Assert
        Assert.Equal(name, group.Name);
        Assert.Equal(description, group.Description);
        Assert.Empty(group.Users);
        Assert.Empty(group.AllowedFeatures);
        Assert.NotEqual(Guid.Empty, group.Id);
    }

    [Fact]
    public void AddUser_Should_Add_User_To_Collection()
    {
        // Arrange
        var group = new UserGroup("Group1");
        var user = new UserRoot(new EmailAddress("test@example.com"), "John", "Doe", Guid.NewGuid().ToString());

        // Act
        group.AddUser(user);

        // Assert
        Assert.Contains(user, group.Users);
    }

    [Fact]
    public void RemoveUser_Should_Remove_User_From_Collection()
    {
        // Arrange
        var group = new UserGroup("Group1");
        var user = new UserRoot(new EmailAddress("test@example.com"), "John", "Doe", Guid.NewGuid().ToString());
        group.AddUser(user);

        // Act
        group.RemoveUser(user);

        // Assert
        Assert.DoesNotContain(user, group.Users);
    }

    [Fact]
    public void AddFeature_Should_Add_Feature()
    {
        // Arrange
        var group = new UserGroup("Group1");
        var feature = "Feature1";

        // Act
        group.AddFeature(feature);

        // Assert
        Assert.Contains(feature, group.AllowedFeatures);
    }
}

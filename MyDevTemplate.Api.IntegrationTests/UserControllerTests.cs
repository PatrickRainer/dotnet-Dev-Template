using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using MyDevTemplate.Application.UserServices.Dtos;
using MyDevTemplate.Domain.Entities.UserAggregate;

namespace MyDevTemplate.Api.IntegrationTests;

public class UserControllerTests : IntegrationTestBase
{
    public UserControllerTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task UserLifecycle_ShouldSucceed()
    {
        // Arrange
        var userEmail = $"john.doe.{Guid.NewGuid()}@example.com";
        var addUserDto = new AddUserDto(
            FirstName: "John",
            LastName: "Doe",
            Email: userEmail,
            IdentityProviderId: "auth0|integration-test"
        );

        // Act & Assert: 1. Add User
        var postResponse = await Client.PostAsJsonAsync("/api/v1/User", addUserDto);
        Assert.Equal(HttpStatusCode.OK, postResponse.StatusCode);

        // Act & Assert: 2. Get User by Email
        var getByEmailResponse = await Client.GetAsync($"/api/v1/User/email/{userEmail}");
        Assert.Equal(HttpStatusCode.OK, getByEmailResponse.StatusCode);
        var user = await getByEmailResponse.Content.ReadFromJsonAsync<UserRoot>();
        Assert.NotNull(user);
        var userId = user.Id;

        // Act & Assert: 3. Get User by Id
        var getByIdResponse = await Client.GetAsync($"/api/v1/User/{userId}");
        Assert.Equal(HttpStatusCode.OK, getByIdResponse.StatusCode);

        // Act & Assert: 4. Get All Users
        var getAllResponse = await Client.GetAsync("/api/v1/User");
        Assert.Equal(HttpStatusCode.OK, getAllResponse.StatusCode);
        var users = await getAllResponse.Content.ReadFromJsonAsync<IEnumerable<UserRoot>>();
        Assert.NotNull(users);
        Assert.Contains(users, u => u.Id == userId);

        // Act & Assert: 5. Update User
        var updateUserDto = new UpdateUserDto("Jane", "Smith");
        var putResponse = await Client.PutAsJsonAsync($"/api/v1/User/{userId}", updateUserDto);
        Assert.Equal(HttpStatusCode.OK, putResponse.StatusCode);

        // Verify Update
        var getAfterUpdateResponse = await Client.GetAsync($"/api/v1/User/{userId}");
        var updatedUser = await getAfterUpdateResponse.Content.ReadFromJsonAsync<UserRoot>();
        Assert.NotNull(updatedUser);
        Assert.Equal("Jane", updatedUser.FirstName);
        Assert.Equal("Smith", updatedUser.LastName);

        // Act & Assert: 6. Remove User by Id
        var deleteResponse = await Client.DeleteAsync($"/api/v1/User/{userId}");
        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

        // Act & Assert: 7. Verify User is gone
        var getResponseAfterDelete = await Client.GetAsync($"/api/v1/User/{userId}");
        Assert.Equal(HttpStatusCode.NotFound, getResponseAfterDelete.StatusCode);
    }

    [Fact]
    public async Task RemoveUserByEmail_ShouldSucceed()
    {
        // Arrange
        var userEmail = $"remove.by.email.{Guid.NewGuid()}@example.com";
        var addUserDto = new AddUserDto("Remove", "Me", userEmail, "oid");
        await Client.PostAsJsonAsync("/api/v1/User", addUserDto);

        // Act
        var deleteResponse = await Client.DeleteAsync($"/api/v1/User/email/{userEmail}");
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
        var getResponse = await Client.GetAsync($"/api/v1/User/email/{userEmail}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }
}

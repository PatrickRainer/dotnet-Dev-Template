using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using MyDevTemplate.Api.Controllers;
using MyDevTemplate.Application.UserServices.Dtos;

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
        var userEmail = "john.doe.integration@example.com";
        var addUserDto = new AddUserDto(
            FirstName: "John",
            LastName: "Doe",
            Email: userEmail,
            IdentityProviderId: "auth0|integration-test"
        );

        // Act & Assert: 1. Add User
        var postResponse = await Client.PostAsJsonAsync("/api/v1/User", addUserDto);
        Assert.Equal(HttpStatusCode.OK, postResponse.StatusCode);

        // Act & Assert: 2. Get User
        var getResponse = await Client.GetAsync($"/api/v1/User/{userEmail}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        
        // Act & Assert: 3. Remove User
        var deleteResponse = await Client.DeleteAsync($"/api/v1/User/{userEmail}");
        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

        // Act & Assert: 4. Verify User is gone
        var getResponseAfterDelete = await Client.GetAsync($"/api/v1/User/{userEmail}");
        Assert.Equal(HttpStatusCode.NotFound, getResponseAfterDelete.StatusCode);
    }
}

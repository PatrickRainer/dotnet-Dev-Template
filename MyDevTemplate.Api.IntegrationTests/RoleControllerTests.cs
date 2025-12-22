using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using MyDevTemplate.Api.Controllers;

namespace MyDevTemplate.Api.IntegrationTests;

public class RoleControllerTests : IntegrationTestBase
{
    public RoleControllerTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task RoleLifecycle_ShouldSucceed()
    {
        // Arrange
        var addRoleDto = new AddRoleDto(
            Title: "Integration Test Role",
            Description: "Role created during integration testing",
            TenantId: "3fa85f64-5717-4562-b3fc-2c963f66afa6"
        );

        // Act & Assert: 1. Add Role
        var postResponse = await Client.PostAsJsonAsync("/api/v1/Role", addRoleDto);
        Assert.Equal(HttpStatusCode.OK, postResponse.StatusCode);
        var roleId = await postResponse.Content.ReadFromJsonAsync<Guid>();
        Assert.NotEqual(Guid.Empty, roleId);

        // Act & Assert: 2. Get All Roles
        var getAllResponse = await Client.GetAsync("/api/v1/Role");
        Assert.Equal(HttpStatusCode.OK, getAllResponse.StatusCode);

        // Act & Assert: 3. Get Role by ID
        var getByIdResponse = await Client.GetAsync($"/api/v1/Role/{roleId}");
        Assert.Equal(HttpStatusCode.OK, getByIdResponse.StatusCode);
        
        // Act & Assert: 4. Update Role
        var updateRoleDto = new UpdateRoleDto(
            Title: "Updated Integration Test Role",
            Description: "Updated description during integration testing"
        );
        var putResponse = await Client.PutAsJsonAsync($"/api/v1/Role/{roleId}", updateRoleDto);
        Assert.Equal(HttpStatusCode.OK, putResponse.StatusCode);

        // Act & Assert: 5. Remove Role
        var deleteResponse = await Client.DeleteAsync($"/api/v1/Role/{roleId}");
        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

        // Act & Assert: 6. Verify Role is gone
        var getAfterDeleteResponse = await Client.GetAsync($"/api/v1/Role/{roleId}");
        Assert.Equal(HttpStatusCode.NotFound, getAfterDeleteResponse.StatusCode);
    }
}

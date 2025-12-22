using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using MyDevTemplate.Api.Controllers;

namespace MyDevTemplate.Api.IntegrationTests;

public class UserControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    readonly WebApplicationFactory<Program> _factory;
    readonly HttpClient _client;
    readonly string _apiKey;
    readonly string _tenantId;

    public UserControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        _apiKey = configuration["IntegrationTests:ApiKey"] ?? throw new InvalidOperationException("ApiKey not found in configuration");
        _tenantId = configuration["IntegrationTests:TenantId"] ?? throw new InvalidOperationException("TenantId not found in configuration");

        _client = _factory.CreateClient();
        _client.DefaultRequestHeaders.Add("X-Api-Key", _apiKey);
        _client.DefaultRequestHeaders.Add("X-Tenant-Id", _tenantId);
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
            IdentityProviderId: "auth0|integration-test",
            TenantId: "3fa85f64-5717-4562-b3fc-2c963f66afa6"
        );

        // Act & Assert: 1. Add User
        var postResponse = await _client.PostAsJsonAsync("/api/v1/User", addUserDto);
        Assert.Equal(HttpStatusCode.OK, postResponse.StatusCode);

        // Act & Assert: 2. Get User
        var getResponse = await _client.GetAsync($"/api/v1/User/{userEmail}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        
        // Act & Assert: 3. Remove User
        var deleteResponse = await _client.DeleteAsync($"/api/v1/User/{userEmail}");
        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

        // Act & Assert: 4. Verify User is gone
        var getResponseAfterDelete = await _client.GetAsync($"/api/v1/User/{userEmail}");
        Assert.Equal(HttpStatusCode.NotFound, getResponseAfterDelete.StatusCode);
    }
}

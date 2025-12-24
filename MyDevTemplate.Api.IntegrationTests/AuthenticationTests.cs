using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using MyDevTemplate.Api.Controllers;
using MyDevTemplate.Application.ApiKeyServices.Dtos;

namespace MyDevTemplate.Api.IntegrationTests;

public class AuthenticationTests : IntegrationTestBase
{
    public AuthenticationTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task Request_WithoutApiKey_ShouldReturn401Unauthorized()
    {
        // Arrange
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Tenant-Id", TenantId);

        // Act
        var response = await client.GetAsync("/api/v1/User/email/test@example.com");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Request_WithInvalidApiKey_ShouldReturn401Unauthorized()
    {
        // Arrange
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Api-Key", "InvalidKey");
        client.DefaultRequestHeaders.Add("X-Tenant-Id", TenantId);

        // Act
        var response = await client.GetAsync("/api/v1/User/email/test@example.com");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Request_WithoutTenantId_ShouldReturn401Unauthorized()
    {
        // Arrange
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Api-Key", ApiKey);

        // Act
        var response = await client.GetAsync("/api/v1/User/email/test@example.com");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task MasterTenantPolicy_WithValidMasterKey_ShouldSucceed()
    {
        // Act - TenantController is protected by MasterTenant policy
        var response = await Client.GetAsync("/api/v1/Tenant");

        // Assert - Should not be 401 or 403. Might be 200 OK.
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task DynamicApiKey_LifecycleAndAccess()
    {
        // 1. Create a dynamic API Key using Master Key
        var newTenantId = Guid.NewGuid().ToString();
        var dynamicKey = $"dynamic-key-{Guid.NewGuid()}";
        var addApiKeyDto = new AddApiKeyDto(
            Key: dynamicKey,
            Label: "Dynamic Test Key",
            ExpiresAtUtc: DateTime.UtcNow.AddDays(1),
            TenantId: newTenantId
        );

        var createKeyResponse = await Client.PostAsJsonAsync("/api/v1/ApiKey", addApiKeyDto);
        Assert.Equal(HttpStatusCode.OK, createKeyResponse.StatusCode);
        var apiKeyId = await createKeyResponse.Content.ReadFromJsonAsync<Guid>();

        // 2. Use the dynamic API Key to access UserController (should succeed)
        var dynamicClient = Factory.CreateClient();
        dynamicClient.DefaultRequestHeaders.Add("X-Api-Key", dynamicKey);
        dynamicClient.DefaultRequestHeaders.Add("X-Tenant-Id", newTenantId);

        var userResponse = await dynamicClient.GetAsync("/api/v1/User/email/nonexistent@example.com");
        // Should be 404 because user doesn't exist, but NOT 401/403
        Assert.Equal(HttpStatusCode.NotFound, userResponse.StatusCode);

        // 3. Use the dynamic API Key to access TenantController (should fail with 403 Forbidden because it's not the Master Key)
        var tenantResponse = await dynamicClient.GetAsync("/api/v1/Tenant");
        Assert.Equal(HttpStatusCode.Forbidden, tenantResponse.StatusCode);

        // Cleanup: Remove the dynamic API Key
        var deleteResponse = await Client.DeleteAsync($"/api/v1/ApiKey/{apiKeyId}");
        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
    }
}

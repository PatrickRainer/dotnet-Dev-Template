using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using MyDevTemplate.Api.Controllers;
using MyDevTemplate.Application.ApiKeyServices.Dtos;

namespace MyDevTemplate.Api.IntegrationTests;

public class ApiKeyControllerTests : IntegrationTestBase
{
    public ApiKeyControllerTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task ApiKeyLifecycle_ShouldSucceed()
    {
        // Arrange
        var testTenantId = Guid.NewGuid().ToString();
        var addApiKeyDto = new AddApiKeyDto(
            Key: $"test-key-{Guid.NewGuid()}",
            Label: "Integration Test Key",
            ExpiresAtUtc: DateTime.UtcNow.AddDays(7),
            TenantId: testTenantId
        );

        // Act & Assert: 1. Add API Key
        // Using the default Client (Master Key) to create an API key for another tenant
        var postResponse = await Client.PostAsJsonAsync("/api/v1/ApiKey", addApiKeyDto);
        Assert.Equal(HttpStatusCode.OK, postResponse.StatusCode);
        var apiKeyId = await postResponse.Content.ReadFromJsonAsync<Guid>();
        Assert.NotEqual(Guid.Empty, apiKeyId);

        // Act & Assert: 2. Get API Keys for Tenant
        // We use a client impersonating that tenant to see THEIR API keys
        var tenantClient = Factory.CreateClient();
        tenantClient.DefaultRequestHeaders.Add("X-Api-Key", ApiKey); // Still Master Key
        tenantClient.DefaultRequestHeaders.Add("X-Tenant-Id", testTenantId);

        var getResponse = await tenantClient.GetAsync("/api/v1/ApiKey");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var apiKeys = await getResponse.Content.ReadFromJsonAsync<IEnumerable<ApiKeyResponse>>();
        Assert.NotNull(apiKeys);
        Assert.Contains(apiKeys, k => k.Label == addApiKeyDto.Label);

        // Act & Assert: 3. Remove API Key
        var deleteResponse = await Client.DeleteAsync($"/api/v1/ApiKey/{apiKeyId}");
        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

        // Act & Assert: 4. Verify API Key is gone
        var getResponseAfterDelete = await tenantClient.GetAsync("/api/v1/ApiKey");
        Assert.Equal(HttpStatusCode.OK, getResponseAfterDelete.StatusCode);
        var apiKeysAfterDelete = await getResponseAfterDelete.Content.ReadFromJsonAsync<IEnumerable<ApiKeyResponse>>();
        Assert.NotNull(apiKeysAfterDelete);
        Assert.DoesNotContain(apiKeysAfterDelete, k => k.Id == apiKeyId);
    }

    private record ApiKeyResponse(Guid Id, string Label);
}

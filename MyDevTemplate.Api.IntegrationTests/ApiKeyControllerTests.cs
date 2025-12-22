using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using MyDevTemplate.Api.Controllers;

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
        var tenantId = "3fa85f64-5717-4562-b3fc-2c963f66afa6";
        var addApiKeyDto = new AddApiKeyDto(
            Key: $"test-key-{Guid.NewGuid()}",
            Label: "Integration Test Key",
            TenantId: tenantId,
            ExpiresAtUtc: DateTime.UtcNow.AddDays(7)
        );

        // Act & Assert: 1. Add API Key
        var postResponse = await Client.PostAsJsonAsync("/api/v1/ApiKey", addApiKeyDto);
        Assert.Equal(HttpStatusCode.OK, postResponse.StatusCode);
        var apiKeyId = await postResponse.Content.ReadFromJsonAsync<Guid>();
        Assert.NotEqual(Guid.Empty, apiKeyId);

        // Act & Assert: 2. Get API Keys for Tenant
        var getResponse = await Client.GetAsync($"/api/v1/ApiKey/tenant/{tenantId}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var apiKeys = await getResponse.Content.ReadFromJsonAsync<IEnumerable<ApiKeyResponse>>();
        Assert.NotNull(apiKeys);
        Assert.Contains(apiKeys, k => k.Label == addApiKeyDto.Label);

        // Act & Assert: 3. Remove API Key
        var deleteResponse = await Client.DeleteAsync($"/api/v1/ApiKey/{apiKeyId}");
        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

        // Act & Assert: 4. Verify API Key is gone (optional, but good practice)
        var getResponseAfterDelete = await Client.GetAsync($"/api/v1/ApiKey/tenant/{tenantId}");
        Assert.Equal(HttpStatusCode.OK, getResponseAfterDelete.StatusCode);
        var apiKeysAfterDelete = await getResponseAfterDelete.Content.ReadFromJsonAsync<IEnumerable<ApiKeyResponse>>();
        Assert.NotNull(apiKeysAfterDelete);
        Assert.DoesNotContain(apiKeysAfterDelete, k => k.Id == apiKeyId);
    }

    private record ApiKeyResponse(Guid Id, string Label);
}

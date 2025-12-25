using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using MyDevTemplate.Application.ApiKeyServices.Dtos;
using MyDevTemplate.Domain.Entities.ApiKeyAggregate;

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
        var postResponse = await Client.PostAsJsonAsync("/api/v1/ApiKey", addApiKeyDto);
        Assert.Equal(HttpStatusCode.OK, postResponse.StatusCode);
        var apiKeyId = await postResponse.Content.ReadFromJsonAsync<Guid>();
        Assert.NotEqual(Guid.Empty, apiKeyId);

        // Act & Assert: 2. Get API Key by Id
        var getByIdResponse = await Client.GetAsync($"/api/v1/ApiKey/{apiKeyId}");
        Assert.Equal(HttpStatusCode.OK, getByIdResponse.StatusCode);
        var apiKey = await getByIdResponse.Content.ReadFromJsonAsync<ApiKeyRoot>();
        Assert.NotNull(apiKey);
        Assert.Equal(addApiKeyDto.Label, apiKey.Label);

        // Act & Assert: 3. Get All API Keys for Tenant
        var tenantClient = Factory.CreateClient();
        tenantClient.DefaultRequestHeaders.Add("X-Api-Key", ApiKey); // Still Master Key
        tenantClient.DefaultRequestHeaders.Add("X-Tenant-Id", testTenantId);

        var getResponse = await tenantClient.GetAsync("/api/v1/ApiKey");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var apiKeys = await getResponse.Content.ReadFromJsonAsync<IEnumerable<ApiKeyResponse>>();
        Assert.NotNull(apiKeys);
        Assert.Contains(apiKeys, k => k.Label == addApiKeyDto.Label);

        // Act & Assert: 4. Update API Key
        var updateApiKeyDto = new UpdateApiKeyDto("Updated Label", true, DateTime.UtcNow.AddDays(14));
        var putResponse = await Client.PutAsJsonAsync($"/api/v1/ApiKey/{apiKeyId}", updateApiKeyDto);
        Assert.Equal(HttpStatusCode.OK, putResponse.StatusCode);

        // Verify update
        var getAfterUpdateResponse = await Client.GetAsync($"/api/v1/ApiKey/{apiKeyId}");
        var updatedApiKey = await getAfterUpdateResponse.Content.ReadFromJsonAsync<ApiKeyRoot>();
        Assert.NotNull(updatedApiKey);
        Assert.Equal("Updated Label", updatedApiKey.Label);

        // Act & Assert: 5. Remove API Key
        var deleteResponse = await Client.DeleteAsync($"/api/v1/ApiKey/{apiKeyId}");
        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

        // Act & Assert: 6. Verify API Key is gone
        var getResponseAfterDelete = await tenantClient.GetAsync("/api/v1/ApiKey");
        Assert.Equal(HttpStatusCode.OK, getResponseAfterDelete.StatusCode);
        var apiKeysAfterDelete = await getResponseAfterDelete.Content.ReadFromJsonAsync<IEnumerable<ApiKeyResponse>>();
        Assert.NotNull(apiKeysAfterDelete);
        Assert.DoesNotContain(apiKeysAfterDelete, k => k.Id == apiKeyId);
    }

    private record ApiKeyResponse(Guid Id, string Label);
}

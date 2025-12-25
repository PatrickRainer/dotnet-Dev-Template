using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using MyDevTemplate.Application.ApiKeyServices.Dtos;
using MyDevTemplate.Domain.Entities.ApiKeyAggregate;

namespace MyDevTemplate.Api.IntegrationTests;

public class ApiKeyIsolationTests : IntegrationTestBase
{
    public ApiKeyIsolationTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task RegularTenant_CreatingApiKey_ShouldHaveTheirOwnTenantId()
    {
        // 1. Create a tenant and an API key for them using Master Key
        var tenantId = Guid.NewGuid();
        var initialKey = $"initial-key-{Guid.NewGuid()}";
        await CreateApiKeyForTenant(tenantId, initialKey);

        // 2. Use the tenant's API key to create a NEW API key
        var tenantClient = Factory.CreateClient();
        tenantClient.DefaultRequestHeaders.Add("X-Api-Key", initialKey);
        tenantClient.DefaultRequestHeaders.Add("X-Tenant-Id", tenantId.ToString());

        var newKey = $"new-key-{Guid.NewGuid()}";
        var addApiKeyDto = new AddApiKeyDto(newKey, "Secondary Key", null, null);

        var response = await tenantClient.PostAsJsonAsync("/api/v1/ApiKey", addApiKeyDto);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var newKeyId = await response.Content.ReadFromJsonAsync<Guid>();

        // 3. Verify the new key belongs to the tenant, even if they didn't specify it
        // We use the Master Key to check the record in the DB (via API)
        var getResponse = await Client.GetAsync($"/api/v1/ApiKey/{newKeyId}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var apiKeyRecord = await getResponse.Content.ReadFromJsonAsync<ApiKeyRoot>();
        
        Assert.NotNull(apiKeyRecord);
        Assert.Equal(tenantId, apiKeyRecord.TenantId);
        Assert.Equal(newKey, apiKeyRecord.Key);

        // 4. Try to create a key for ANOTHER tenant (should be overwritten by AppDbContext)
        var otherTenantId = Guid.NewGuid();
        var maliciousDto = new AddApiKeyDto($"malicious-{Guid.NewGuid()}", "Malicious Key", null, otherTenantId.ToString());
        
        var maliciousResponse = await tenantClient.PostAsJsonAsync("/api/v1/ApiKey", maliciousDto);
        Assert.Equal(HttpStatusCode.OK, maliciousResponse.StatusCode);
        var maliciousKeyId = await maliciousResponse.Content.ReadFromJsonAsync<Guid>();

        var getMaliciousResponse = await Client.GetAsync($"/api/v1/ApiKey/{maliciousKeyId}");
        var maliciousRecord = await getMaliciousResponse.Content.ReadFromJsonAsync<ApiKeyRoot>();
        
        Assert.NotNull(maliciousRecord);
        Assert.Equal(tenantId, maliciousRecord.TenantId); // Should be the creator's tenant, not the one in DTO
        Assert.NotEqual(otherTenantId, maliciousRecord.TenantId);
    }

    [Fact]
    public async Task RegularTenant_ListApiKeys_ShouldOnlySeeTheirOwn()
    {
        // 1. Setup two tenants with their own API keys
        var tenantAId = Guid.NewGuid();
        var tenantBId = Guid.NewGuid();
        var apiKeyA = $"list-key-a-{Guid.NewGuid()}";
        var apiKeyB = $"list-key-b-{Guid.NewGuid()}";

        await CreateApiKeyForTenant(tenantAId, apiKeyA);
        await CreateApiKeyForTenant(tenantBId, apiKeyB);

        // 2. Create clients
        var clientA = Factory.CreateClient();
        clientA.DefaultRequestHeaders.Add("X-Api-Key", apiKeyA);
        clientA.DefaultRequestHeaders.Add("X-Tenant-Id", tenantAId.ToString());

        var clientB = Factory.CreateClient();
        clientB.DefaultRequestHeaders.Add("X-Api-Key", apiKeyB);
        clientB.DefaultRequestHeaders.Add("X-Tenant-Id", tenantBId.ToString());

        // 3. Verify Isolation
        var listA = await clientA.GetFromJsonAsync<IEnumerable<ApiKeyRoot>>("/api/v1/ApiKey");
        Assert.NotNull(listA);
        Assert.Contains(listA, k => k.Key == apiKeyA);
        Assert.DoesNotContain(listA, k => k.Key == apiKeyB);

        var listB = await clientB.GetFromJsonAsync<IEnumerable<ApiKeyRoot>>("/api/v1/ApiKey");
        Assert.NotNull(listB);
        Assert.Contains(listB, k => k.Key == apiKeyB);
        Assert.DoesNotContain(listB, k => k.Key == apiKeyA);
    }

    [Fact]
    public async Task RegularTenant_GetAnotherTenantsApiKeyById_ShouldReturnNotFound()
    {
        // 1. Setup two tenants
        var tenantAId = Guid.NewGuid();
        var tenantBId = Guid.NewGuid();
        var apiKeyA = $"key-a-{Guid.NewGuid()}";
        var apiKeyB = $"key-b-{Guid.NewGuid()}";

        await CreateApiKeyForTenant(tenantAId, apiKeyA);
        await CreateApiKeyForTenant(tenantBId, apiKeyB);

        // Get the ID of Tenant B's API key using Master Key
        var listBResponse = await Client.GetFromJsonAsync<IEnumerable<ApiKeyRoot>>($"/api/v1/ApiKey");
        var apiKeyBId = listBResponse!.First(k => k.Key == apiKeyB).Id;

        // 2. Create client for Tenant A
        var clientA = Factory.CreateClient();
        clientA.DefaultRequestHeaders.Add("X-Api-Key", apiKeyA);
        clientA.DefaultRequestHeaders.Add("X-Tenant-Id", tenantAId.ToString());

        // 3. Try to get Tenant B's key using Tenant A's client
        var response = await clientA.GetAsync($"/api/v1/ApiKey/{apiKeyBId}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RegularTenant_UpdateAnotherTenantsApiKey_ShouldReturnNotFound()
    {
        // 1. Setup two tenants
        var tenantAId = Guid.NewGuid();
        var tenantBId = Guid.NewGuid();
        var apiKeyA = $"key-a-{Guid.NewGuid()}";
        var apiKeyB = $"key-b-{Guid.NewGuid()}";

        await CreateApiKeyForTenant(tenantAId, apiKeyA);
        await CreateApiKeyForTenant(tenantBId, apiKeyB);

        // Get the ID of Tenant B's API key
        var listBResponse = await Client.GetFromJsonAsync<IEnumerable<ApiKeyRoot>>($"/api/v1/ApiKey");
        var apiKeyBId = listBResponse!.First(k => k.Key == apiKeyB).Id;

        // 2. Create client for Tenant A
        var clientA = Factory.CreateClient();
        clientA.DefaultRequestHeaders.Add("X-Api-Key", apiKeyA);
        clientA.DefaultRequestHeaders.Add("X-Tenant-Id", tenantAId.ToString());

        // 3. Try to update Tenant B's key
        var updateDto = new UpdateApiKeyDto("Malicious Update", true, null);
        var response = await clientA.PutAsJsonAsync($"/api/v1/ApiKey/{apiKeyBId}", updateDto);
        
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RegularTenant_DeleteAnotherTenantsApiKey_ShouldReturnNotFound()
    {
        // 1. Setup two tenants
        var tenantAId = Guid.NewGuid();
        var tenantBId = Guid.NewGuid();
        var apiKeyA = $"key-a-{Guid.NewGuid()}";
        var apiKeyB = $"key-b-{Guid.NewGuid()}";

        await CreateApiKeyForTenant(tenantAId, apiKeyA);
        await CreateApiKeyForTenant(tenantBId, apiKeyB);

        // Get the ID of Tenant B's API key
        var listBResponse = await Client.GetFromJsonAsync<IEnumerable<ApiKeyRoot>>($"/api/v1/ApiKey");
        var apiKeyBId = listBResponse!.First(k => k.Key == apiKeyB).Id;

        // 2. Create client for Tenant A
        var clientA = Factory.CreateClient();
        clientA.DefaultRequestHeaders.Add("X-Api-Key", apiKeyA);
        clientA.DefaultRequestHeaders.Add("X-Tenant-Id", tenantAId.ToString());

        // 3. Try to delete Tenant B's key
        var response = await clientA.DeleteAsync($"/api/v1/ApiKey/{apiKeyBId}");
        
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task CreateApiKeyForTenant(Guid tenantId, string key)
    {
        var dto = new AddApiKeyDto(key, $"Key for {tenantId}", DateTime.UtcNow.AddDays(1), tenantId.ToString());
        var response = await Client.PostAsJsonAsync("/api/v1/ApiKey", dto);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}

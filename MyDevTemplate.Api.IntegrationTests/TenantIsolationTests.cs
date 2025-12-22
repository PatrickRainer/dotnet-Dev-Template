using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using MyDevTemplate.Api.Controllers;

namespace MyDevTemplate.Api.IntegrationTests;

public class TenantIsolationTests : IntegrationTestBase
{
    public TenantIsolationTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task MultiTenantIsolation_ShouldRestrictAccessToOwnedRecords()
    {
        // 1. Setup two tenants with their own API keys
        var tenantAId = Guid.NewGuid();
        var tenantBId = Guid.NewGuid();
        var apiKeyA = $"key-a-{Guid.NewGuid()}";
        var apiKeyB = $"key-b-{Guid.NewGuid()}";

        // Create API keys for tenants (using Master Key)
        await CreateApiKeyForTenant(tenantAId, apiKeyA);
        await CreateApiKeyForTenant(tenantBId, apiKeyB);

        var userAEmail = string.Empty;
        var userBEmail = string.Empty;
        try
        {
            // 2. Create clients for each tenant
            var clientA = CreateTenantClient(tenantAId, apiKeyA);
            var clientB = CreateTenantClient(tenantBId, apiKeyB);

            // 3. Create a unique user in Tenant A
            userAEmail = $"user-a-{Guid.NewGuid()}@example.com";
            var addUserADto = new AddUserDto("User", "A", userAEmail, "auth0|a");
            var responseA = await clientA.PostAsJsonAsync("/api/v1/User", addUserADto);
            Assert.Equal(HttpStatusCode.OK, responseA.StatusCode);

            // 4. Create a unique user in Tenant B
            userBEmail = $"user-b-{Guid.NewGuid()}@example.com";
            var addUserBDto = new AddUserDto("User", "B", userBEmail, "auth0|b");
            var responseB = await clientB.PostAsJsonAsync("/api/v1/User", addUserBDto);
            Assert.Equal(HttpStatusCode.OK, responseB.StatusCode);

            // 5. Verify Isolation
            
            // Tenant A should see User A
            var getAByA = await clientA.GetAsync($"/api/v1/User/{userAEmail}");
            Assert.Equal(HttpStatusCode.OK, getAByA.StatusCode);

            // Tenant A should NOT see User B
            var getBByA = await clientA.GetAsync($"/api/v1/User/{userBEmail}");
            Assert.Equal(HttpStatusCode.NotFound, getBByA.StatusCode);

            // Tenant B should see User B
            var getBByB = await clientB.GetAsync($"/api/v1/User/{userBEmail}");
            Assert.Equal(HttpStatusCode.OK, getBByB.StatusCode);

            // Tenant B should NOT see User A
            var getAByB = await clientB.GetAsync($"/api/v1/User/{userAEmail}");
            Assert.Equal(HttpStatusCode.NotFound, getAByB.StatusCode);

            // Master Tenant (Client) should see both (since Master Key bypasses filters)
            var getAByMaster = await Client.GetAsync($"/api/v1/User/{userAEmail}");
            Assert.Equal(HttpStatusCode.OK, getAByMaster.StatusCode);
            
            var getBByMaster = await Client.GetAsync($"/api/v1/User/{userBEmail}");
            Assert.Equal(HttpStatusCode.OK, getBByMaster.StatusCode);
        }
        finally
        {
            // Cleanup (using Master Key)
            await Client.DeleteAsync($"/api/v1/User/{userAEmail}");
            await Client.DeleteAsync($"/api/v1/User/{userBEmail}");
        }
    }

    [Fact]
    public async Task MasterTenant_ShouldSeeAllRecordsAcrossTenants()
    {
        // 1. Setup two tenants with their own API keys
        var tenantAId = Guid.NewGuid();
        var tenantBId = Guid.NewGuid();
        var apiKeyA = $"key-a-{Guid.NewGuid()}";
        var apiKeyB = $"key-b-{Guid.NewGuid()}";

        await CreateApiKeyForTenant(tenantAId, apiKeyA);
        await CreateApiKeyForTenant(tenantBId, apiKeyB);

        var userAEmail = $"master-test-a-{Guid.NewGuid()}@example.com";
        var userBEmail = $"master-test-b-{Guid.NewGuid()}@example.com";

        try
        {
            var clientA = CreateTenantClient(tenantAId, apiKeyA);
            var clientB = CreateTenantClient(tenantBId, apiKeyB);

            // 2. Create users in different tenants
            await clientA.PostAsJsonAsync("/api/v1/User", new AddUserDto("Master", "A", userAEmail, "auth0|ma"));
            await clientB.PostAsJsonAsync("/api/v1/User", new AddUserDto("Master", "B", userBEmail, "auth0|mb"));

            // 3. Verify Master Key can see both
            var responseA = await Client.GetAsync($"/api/v1/User/{userAEmail}");
            var responseB = await Client.GetAsync($"/api/v1/User/{userBEmail}");

            Assert.Equal(HttpStatusCode.OK, responseA.StatusCode);
            Assert.Equal(HttpStatusCode.OK, responseB.StatusCode);
            
            // 4. Verify Master Key can list both (if we had a list endpoint, but we use Get by email for now)
            // The existing assertions in Step 3 already prove the bypass works for individual records.
        }
        finally
        {
            await Client.DeleteAsync($"/api/v1/User/{userAEmail}");
            await Client.DeleteAsync($"/api/v1/User/{userBEmail}");
        }
    }

    async Task CreateApiKeyForTenant(Guid tenantId, string key)
    {
        var dto = new AddApiKeyDto(key, $"Key for {tenantId}", DateTime.UtcNow.AddDays(1), tenantId.ToString());
        var response = await Client.PostAsJsonAsync("/api/v1/ApiKey", dto);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    HttpClient CreateTenantClient(Guid tenantId, string apiKey)
    {
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);
        client.DefaultRequestHeaders.Add("X-Tenant-Id", tenantId.ToString());
        return client;
    }
}

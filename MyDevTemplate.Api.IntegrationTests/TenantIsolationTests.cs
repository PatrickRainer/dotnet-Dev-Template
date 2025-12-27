using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using MyDevTemplate.Application.ApiKeyServices.Dtos;
using MyDevTemplate.Application.UserServices.Dtos;
using MyDevTemplate.Domain.Entities.ApiKeyAggregate;
using MyDevTemplate.Domain.Entities.UserAggregate;

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

            // 5. Verify Isolation (Single records)
            
            // Tenant A should see User A
            var getAByA = await clientA.GetAsync($"/api/v1/User/email/{userAEmail}");
            Assert.Equal(HttpStatusCode.OK, getAByA.StatusCode);

            // Tenant A should NOT see User B
            var getBByA = await clientA.GetAsync($"/api/v1/User/email/{userBEmail}");
            Assert.Equal(HttpStatusCode.NotFound, getBByA.StatusCode);

            // Tenant B should see User B
            var getBByB = await clientB.GetAsync($"/api/v1/User/email/{userBEmail}");
            Assert.Equal(HttpStatusCode.OK, getBByB.StatusCode);

            // Tenant B should NOT see User A
            var getAByB = await clientB.GetAsync($"/api/v1/User/email/{userAEmail}");
            Assert.Equal(HttpStatusCode.NotFound, getAByB.StatusCode);

            // Tenant A should NOT see User B by ID
            var userB = await Client.GetFromJsonAsync<UserRoot>($"/api/v1/User/email/{userBEmail}");
            Assert.NotNull(userB);
            var getBByIdByA = await clientA.GetAsync($"/api/v1/User/{userB.Id}");
            Assert.Equal(HttpStatusCode.NotFound, getBByIdByA.StatusCode);

            // 6. Verify Isolation (List records)
            
            // Tenant A should only see User A in the list
            var listA = await clientA.GetFromJsonAsync<IEnumerable<UserRoot>>("/api/v1/User");
            Assert.NotNull(listA);
            Assert.Contains(listA, u => u.Email.Value == userAEmail);
            Assert.DoesNotContain(listA, u => u.Email.Value == userBEmail);

            // Tenant B should only see User B in the list
            var listB = await clientB.GetFromJsonAsync<IEnumerable<UserRoot>>("/api/v1/User");
            Assert.NotNull(listB);
            Assert.Contains(listB, u => u.Email.Value == userBEmail);
            Assert.DoesNotContain(listB, u => u.Email.Value == userAEmail);

            // 7. Master Tenant (Client) should see both (since Master Key bypasses filters)
            var getAByMaster = await Client.GetAsync($"/api/v1/User/email/{userAEmail}");
            Assert.Equal(HttpStatusCode.OK, getAByMaster.StatusCode);
            
            var getBByMaster = await Client.GetAsync($"/api/v1/User/email/{userBEmail}");
            Assert.Equal(HttpStatusCode.OK, getBByMaster.StatusCode);

            var listMaster = await Client.GetFromJsonAsync<IEnumerable<UserRoot>>("/api/v1/User");
            Assert.NotNull(listMaster);
            Assert.Contains(listMaster, u => u.Email.Value == userAEmail);
            Assert.Contains(listMaster, u => u.Email.Value == userBEmail);
        }
        finally
        {
            // Cleanup (using Master Key)
            await Client.DeleteAsync($"/api/v1/User/email/{userAEmail}");
            await Client.DeleteAsync($"/api/v1/User/email/{userBEmail}");
            await DeleteApiKeyByKey(apiKeyA);
            await DeleteApiKeyByKey(apiKeyB);
        }
    }

    [Fact]
    public async Task RoleIsolation_GetAll_ShouldOnlyReturnOwnedRecords()
    {
        // 1. Setup two tenants
        var tenantAId = Guid.NewGuid();
        var tenantBId = Guid.NewGuid();
        var apiKeyA = $"key-role-a-{Guid.NewGuid()}";
        var apiKeyB = $"key-role-b-{Guid.NewGuid()}";

        await CreateApiKeyForTenant(tenantAId, apiKeyA);
        await CreateApiKeyForTenant(tenantBId, apiKeyB);

        var roleATitle = string.Empty;
        var roleBTitle = string.Empty;
        Guid roleAId = Guid.Empty;
        Guid roleBId = Guid.Empty;
        try
        {
            var clientA = CreateTenantClient(tenantAId, apiKeyA);
            var clientB = CreateTenantClient(tenantBId, apiKeyB);

            // 2. Create roles in each tenant
            roleATitle = $"Role A {Guid.NewGuid()}";
            var responseA = await clientA.PostAsJsonAsync("/api/v1/Role", new { Title = roleATitle, Description = "Desc A" });
            roleAId = await responseA.Content.ReadFromJsonAsync<Guid>();

            roleBTitle = $"Role B {Guid.NewGuid()}";
            var responseB = await clientB.PostAsJsonAsync("/api/v1/Role", new { Title = roleBTitle, Description = "Desc B" });
            roleBId = await responseB.Content.ReadFromJsonAsync<Guid>();

            // 3. Verify Isolation
            var listA = await clientA.GetFromJsonAsync<IEnumerable<dynamic>>("/api/v1/Role");
            Assert.NotNull(listA);
            // We use dynamic or a local record because we don't want to depend on the exact RoleRoot structure if it's complex
            // but RoleRoot should work too.
            Assert.Contains(listA, r => r.GetProperty("title").GetString() == roleATitle);
            Assert.DoesNotContain(listA, r => r.GetProperty("title").GetString() == roleBTitle);

            var listB = await clientB.GetFromJsonAsync<IEnumerable<dynamic>>("/api/v1/Role");
            Assert.NotNull(listB);
            Assert.Contains(listB, r => r.GetProperty("title").GetString() == roleBTitle);
            Assert.DoesNotContain(listB, r => r.GetProperty("title").GetString() == roleATitle);
        }
        finally
        {
            // Master key cleanup would be needed here if we had a way to identify these roles easily
            if (roleAId != Guid.Empty) await Client.DeleteAsync($"/api/v1/Role/{roleAId}");
            if (roleBId != Guid.Empty) await Client.DeleteAsync($"/api/v1/Role/{roleBId}");
            await DeleteApiKeyByKey(apiKeyA);
            await DeleteApiKeyByKey(apiKeyB);
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
            var responseA = await Client.GetAsync($"/api/v1/User/email/{userAEmail}");
            var responseB = await Client.GetAsync($"/api/v1/User/email/{userBEmail}");

            Assert.Equal(HttpStatusCode.OK, responseA.StatusCode);
            Assert.Equal(HttpStatusCode.OK, responseB.StatusCode);
            
            // 4. Verify Master Key can list both
            var getAllResponse = await Client.GetAsync("/api/v1/User");
            Assert.Equal(HttpStatusCode.OK, getAllResponse.StatusCode);
            var users = await getAllResponse.Content.ReadFromJsonAsync<IEnumerable<UserRoot>>();
            Assert.NotNull(users);
            Assert.Contains(users, u => u.Email.Value == userAEmail);
            Assert.Contains(users, u => u.Email.Value == userBEmail);
        }
        finally
        {
            await Client.DeleteAsync($"/api/v1/User/email/{userAEmail}");
            await Client.DeleteAsync($"/api/v1/User/email/{userBEmail}");
            await DeleteApiKeyByKey(apiKeyA);
            await DeleteApiKeyByKey(apiKeyB);
        }
    }

    async Task DeleteApiKeyByKey(string key)
    {
        var allKeys = await Client.GetFromJsonAsync<IEnumerable<ApiKeyRoot>>("/api/v1/ApiKey");
        var keyEntity = allKeys?.FirstOrDefault(k => k.Key == key);
        if (keyEntity != null)
        {
            await Client.DeleteAsync($"/api/v1/ApiKey/{keyEntity.Id}");
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

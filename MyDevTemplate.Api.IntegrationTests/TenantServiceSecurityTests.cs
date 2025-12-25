using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using MyDevTemplate.Application.ApiKeyServices.Dtos;
using MyDevTemplate.Application.TenantServices.Dtos;
using Xunit;

namespace MyDevTemplate.Api.IntegrationTests;

public class TenantServiceSecurityTests : IntegrationTestBase
{
    public TenantServiceSecurityTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task RegularTenant_CallingTenantEndpoints_ShouldBeForbidden()
    {
        // 1. Setup a regular tenant
        var tenantId = Guid.NewGuid();
        var apiKey = $"key-{Guid.NewGuid()}";
        await CreateApiKeyForTenant(tenantId, apiKey);

        var tenantClient = Factory.CreateClient();
        tenantClient.DefaultRequestHeaders.Add("X-Api-Key", apiKey);
        tenantClient.DefaultRequestHeaders.Add("X-Tenant-Id", tenantId.ToString());

        // 2. Try to call TenantController endpoints (should fail due to policy)
        var getResponse = await tenantClient.GetAsync("/api/v1/Tenant");
        Assert.Equal(HttpStatusCode.Forbidden, getResponse.StatusCode);

        var createDto = new CreateTenantDto("New Tenant", "New Co", "admin@test.com", "Street", "City", "State", "Country", "12345", null);
        var postResponse = await tenantClient.PostAsJsonAsync("/api/v1/Tenant", createDto);
        Assert.Equal(HttpStatusCode.Forbidden, postResponse.StatusCode);

        var putResponse = await tenantClient.PutAsJsonAsync($"/api/v1/Tenant/{Guid.NewGuid()}", new UpdateTenantDto("Update", "Update Co", "St", "Ci", "St", "Co", "12", null));
        Assert.Equal(HttpStatusCode.Forbidden, putResponse.StatusCode);

        var deleteResponse = await tenantClient.DeleteAsync($"/api/v1/Tenant/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.Forbidden, deleteResponse.StatusCode);
        
        var getByIdResponse = await tenantClient.GetAsync($"/api/v1/Tenant/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.Forbidden, getByIdResponse.StatusCode);
    }

    private async Task CreateApiKeyForTenant(Guid tenantId, string key)
    {
        var dto = new AddApiKeyDto(key, $"Key for {tenantId}", DateTime.UtcNow.AddDays(1), tenantId.ToString());
        var response = await Client.PostAsJsonAsync("/api/v1/ApiKey", dto);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}

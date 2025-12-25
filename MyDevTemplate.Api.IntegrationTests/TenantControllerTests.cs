using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using MyDevTemplate.Api.Controllers;
using MyDevTemplate.Application.TenantServices.Dtos;
using MyDevTemplate.Domain.Entities.TenantAggregate;

namespace MyDevTemplate.Api.IntegrationTests;

public class TenantControllerTests : IntegrationTestBase
{
    public TenantControllerTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task TenantLifecycle_ShouldSucceed()
    {
        // Arrange
        var createTenantDto = new CreateTenantDto(
            TenantName: $"Test Tenant {Guid.NewGuid()}",
            CompanyName: "Test Company",
            Street: "123 Test St",
            City: "Test City",
            State: "TS",
            Country: "Test Country",
            ZipCode: "12345",
            AdminEmail: "TestAdminEmail@MyDevtemplate.com",
            SubscriptionId: null
        );

        // Act & Assert: 1. Create Tenant
        var postResponse = await Client.PostAsJsonAsync("/api/v1/Tenant", createTenantDto);
        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
        var createdTenant = await postResponse.Content.ReadFromJsonAsync<TenantRoot>();
        Assert.NotNull(createdTenant);
        Assert.NotEqual(Guid.Empty, createdTenant.Id);
        Assert.Equal(createTenantDto.TenantName, createdTenant.TenantName);

        // Act & Assert: 2. Get All Tenants
        var getAllResponse = await Client.GetAsync("/api/v1/Tenant");
        Assert.Equal(HttpStatusCode.OK, getAllResponse.StatusCode);
        var tenants = await getAllResponse.Content.ReadFromJsonAsync<IEnumerable<TenantRoot>>();
        Assert.NotNull(tenants);
        Assert.Contains(tenants, t => t.Id == createdTenant.Id);

        // Act & Assert: 3. Get Tenant by ID
        var getByIdResponse = await Client.GetAsync($"/api/v1/Tenant/{createdTenant.Id}");
        Assert.Equal(HttpStatusCode.OK, getByIdResponse.StatusCode);
        var retrievedTenant = await getByIdResponse.Content.ReadFromJsonAsync<TenantRoot>();
        Assert.NotNull(retrievedTenant);
        Assert.Equal(createdTenant.Id, retrievedTenant.Id);

        // Act & Assert: 4. Update Tenant
        var updateTenantDto = new UpdateTenantDto(
            TenantName: $"Updated {createdTenant.TenantName}",
            CompanyName: "Updated Company",
            Street: "456 Updated St",
            City: "Updated City",
            State: "US",
            Country: "Updated Country",
            ZipCode: "54321",
            SubscriptionId: null
        );
        var putResponse = await Client.PutAsJsonAsync($"/api/v1/Tenant/{createdTenant.Id}", updateTenantDto);
        Assert.Equal(HttpStatusCode.NoContent, putResponse.StatusCode);

        // Verify update
        var getAfterUpdateResponse = await Client.GetAsync($"/api/v1/Tenant/{createdTenant.Id}");
        var updatedTenant = await getAfterUpdateResponse.Content.ReadFromJsonAsync<TenantRoot>();
        Assert.NotNull(updatedTenant);
        Assert.Equal(updateTenantDto.TenantName, updatedTenant.TenantName);

        // Act & Assert: 5. Delete Tenant
        var deleteResponse = await Client.DeleteAsync($"/api/v1/Tenant/{createdTenant.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // Act & Assert: 6. Verify Tenant is gone
        var getAfterDeleteResponse = await Client.GetAsync($"/api/v1/Tenant/{createdTenant.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getAfterDeleteResponse.StatusCode);
    }
}

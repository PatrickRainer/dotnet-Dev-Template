using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using MyDevTemplate.Application.ApiKeyServices.Dtos;
using MyDevTemplate.Application.SubscriptionServices.Dtos;
using MyDevTemplate.Domain.Entities.SubscriptionAggregate;

namespace MyDevTemplate.Api.IntegrationTests;

public class SubscriptionControllerTests : IntegrationTestBase
{
    public SubscriptionControllerTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task SubscriptionLifecycle_ShouldSucceed()
    {
        // Arrange
        var createSubscriptionDto = new CreateSubscriptionDto(
            Name: $"Test Subscription {Guid.NewGuid()}",
            Description: "Test Description",
            Features: new List<string> { SubscriptionFeatures.Dashboard, SubscriptionFeatures.Reports }
        );

        // Act & Assert: 1. Create Subscription
        var postResponse = await Client.PostAsJsonAsync("/api/v1/Subscription", createSubscriptionDto);
        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
        var createdSubscription = await postResponse.Content.ReadFromJsonAsync<SubscriptionRoot>();
        Assert.NotNull(createdSubscription);
        Assert.NotEqual(Guid.Empty, createdSubscription.Id);
        Assert.Equal(createSubscriptionDto.Name, createdSubscription.Name);

        // Act & Assert: 2. Get All Subscriptions
        var getAllResponse = await Client.GetAsync("/api/v1/Subscription");
        Assert.Equal(HttpStatusCode.OK, getAllResponse.StatusCode);
        var subscriptions = await getAllResponse.Content.ReadFromJsonAsync<IEnumerable<SubscriptionRoot>>();
        Assert.NotNull(subscriptions);
        Assert.Contains(subscriptions, s => s.Id == createdSubscription.Id);

        // Act & Assert: 3. Get Subscription by ID
        var getByIdResponse = await Client.GetAsync($"/api/v1/Subscription/{createdSubscription.Id}");
        Assert.Equal(HttpStatusCode.OK, getByIdResponse.StatusCode);
        var retrievedSubscription = await getByIdResponse.Content.ReadFromJsonAsync<SubscriptionRoot>();
        Assert.NotNull(retrievedSubscription);
        Assert.Equal(createdSubscription.Id, retrievedSubscription.Id);

        // Act & Assert: 4. Update Subscription
        var updateSubscriptionDto = new UpdateSubscriptionDto(
            Name: $"Updated {createdSubscription.Name}",
            Description: "Updated Description",
            Features: new List<string> { SubscriptionFeatures.Dashboard, SubscriptionFeatures.Reports, SubscriptionFeatures.Settings }
        );
        var putResponse = await Client.PutAsJsonAsync($"/api/v1/Subscription/{createdSubscription.Id}", updateSubscriptionDto);
        Assert.Equal(HttpStatusCode.NoContent, putResponse.StatusCode);

        // Verify update
        var getAfterUpdateResponse = await Client.GetAsync($"/api/v1/Subscription/{createdSubscription.Id}");
        var updatedSubscription = await getAfterUpdateResponse.Content.ReadFromJsonAsync<SubscriptionRoot>();
        Assert.NotNull(updatedSubscription);
        Assert.Equal(updateSubscriptionDto.Name, updatedSubscription.Name);
        Assert.Equal(3, updatedSubscription.Features.Count);

        // Act & Assert: 5. Delete Subscription
        var deleteResponse = await Client.DeleteAsync($"/api/v1/Subscription/{createdSubscription.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // Act & Assert: 6. Verify Subscription is gone
        var getAfterDeleteResponse = await Client.GetAsync($"/api/v1/Subscription/{createdSubscription.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getAfterDeleteResponse.StatusCode);
    }

    [Fact]
    public async Task GetSubscriptionEndpoints_ShouldAllowAnonymous()
    {
        // Arrange
        var anonymousClient = Factory.CreateClient();
        
        // 1. GetAllSubscriptions
        var getAllResponse = await anonymousClient.GetAsync("/api/v1/Subscription");
        Assert.Equal(HttpStatusCode.OK, getAllResponse.StatusCode);
        var subscriptions = await getAllResponse.Content.ReadFromJsonAsync<List<SubscriptionRoot>>();
        Assert.NotNull(subscriptions);
        Assert.NotEmpty(subscriptions); // Should have at least seeded subscriptions

        // 2. GetSubscription by Id
        var firstSubscription = subscriptions.First();
        var getByIdResponse = await anonymousClient.GetAsync($"/api/v1/Subscription/{firstSubscription.Id}");
        Assert.Equal(HttpStatusCode.OK, getByIdResponse.StatusCode);
    }

    [Fact]
    public async Task WriteOperations_ShouldRequireMasterTenant()
    {
        // 1. Setup a regular tenant
        var tenantId = Guid.NewGuid();
        var apiKey = $"key-{Guid.NewGuid()}";
        await CreateApiKeyForTenant(tenantId, apiKey);

        var tenantClient = Factory.CreateClient();
        tenantClient.DefaultRequestHeaders.Add("X-Api-Key", apiKey);
        tenantClient.DefaultRequestHeaders.Add("X-Tenant-Id", tenantId.ToString());

        // 2. Try to call write endpoints (should fail due to policy)
        
        // POST
        var createDto = new CreateSubscriptionDto("Forbidden Subscription", "Desc", new List<string>());
        var postResponse = await tenantClient.PostAsJsonAsync("/api/v1/Subscription", createDto);
        Assert.Equal(HttpStatusCode.Forbidden, postResponse.StatusCode);

        // PUT
        var updateDto = new UpdateSubscriptionDto("Forbidden Update", "Desc", new List<string>());
        var putResponse = await tenantClient.PutAsJsonAsync($"/api/v1/Subscription/{Guid.NewGuid()}", updateDto);
        Assert.Equal(HttpStatusCode.Forbidden, putResponse.StatusCode);

        // DELETE
        var deleteResponse = await tenantClient.DeleteAsync($"/api/v1/Subscription/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.Forbidden, deleteResponse.StatusCode);

        // Cleanup
        var allKeys = await Client.GetFromJsonAsync<IEnumerable<dynamic>>("/api/v1/ApiKey");
        var keyId = allKeys!.First(k => k.GetProperty("key").GetString() == apiKey).GetProperty("id").GetGuid();
        await Client.DeleteAsync($"/api/v1/ApiKey/{keyId}");
    }

    private async Task CreateApiKeyForTenant(Guid tenantId, string key)
    {
        var dto = new AddApiKeyDto(key, $"Key for {tenantId}", DateTime.UtcNow.AddDays(1), tenantId.ToString());
        var response = await Client.PostAsJsonAsync("/api/v1/ApiKey", dto);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}

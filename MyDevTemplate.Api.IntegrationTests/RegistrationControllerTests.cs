using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using MyDevTemplate.Application.RegistrationServices.Dtos;

namespace MyDevTemplate.Api.IntegrationTests;

public class RegistrationControllerTests : IntegrationTestBase
{
    public RegistrationControllerTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task Register_ShouldSucceed_WhenUsingAdminEmailHeader()
    {
        // Arrange
        var registrationDto = new RegistrationDto(
            CompanyName: $"Test Company {Guid.NewGuid()}",
            Street: "123 Registration St",
            City: "Reg City",
            ZipCode: "12345",
            Country: "Reg Country",
            SubscriptionId: null
        );

        var anonymousClient = Factory.CreateClient();
        anonymousClient.DefaultRequestHeaders.Add("X-Admin-Email", "test@admin.com");

        // Act
        var response = await anonymousClient.PostAsJsonAsync("/api/v1/Registration", registrationDto);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<RegistrationResponse>();
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.TenantId);

        // Cleanup
        var deleteResponse = await Client.DeleteAsync($"/api/v1/Tenant/{result.TenantId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task Register_ShouldFail_WhenAnonymous()
    {
        // Arrange
        var registrationDto = new RegistrationDto(
            CompanyName: "Anonymous Co",
            Street: "Street",
            City: "City",
            ZipCode: "12345",
            Country: "Country",
            SubscriptionId: null
        );
        
        var anonymousClient = Factory.CreateClient(); // No headers

        // Act
        var response = await anonymousClient.PostAsJsonAsync("/api/v1/Registration", registrationDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadAsStringAsync();
        Assert.Contains("Admin email is required", error);
    }

    private record RegistrationResponse(Guid TenantId);
}

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace MyDevTemplate.Api.IntegrationTests;

[Collection("IntegrationTests")]
public abstract class IntegrationTestBase : IClassFixture<WebApplicationFactory<Program>>
{
    protected readonly WebApplicationFactory<Program> Factory;
    protected readonly HttpClient Client;
    protected readonly string ApiKey;
    protected readonly string TenantId;

    protected IntegrationTestBase(WebApplicationFactory<Program> factory)
    {
        Factory = factory;
        
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        ApiKey = configuration["IntegrationTests:ApiKey"] ?? throw new InvalidOperationException("ApiKey not found in configuration");
        TenantId = configuration["IntegrationTests:TenantId"] ?? throw new InvalidOperationException("TenantId not found in configuration");

        Client = Factory.CreateClient();
        Client.DefaultRequestHeaders.Add("X-Api-Key", ApiKey);
        Client.DefaultRequestHeaders.Add("X-Tenant-Id", TenantId);
    }
}

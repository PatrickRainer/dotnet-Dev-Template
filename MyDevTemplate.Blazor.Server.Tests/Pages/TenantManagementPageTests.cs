using Bunit;
using Moq;
using MudBlazor;
using MyDevTemplate.Blazor.Server.Components.Pages.Company.TenantManagement;
using MyDevTemplate.Domain.Entities.SubscriptionAggregate;
using MyDevTemplate.Domain.Entities.TenantAggregate;

namespace MyDevTemplate.Blazor.Server.Tests.Pages;

public class TenantManagementPageTests : BlazorTestBase
{
    [Fact]
    public void TenantManagementPage_Should_LoadTenants_OnInitialized()
    {
        // Arrange
        var tenants = new List<TenantRoot>
        {
            new TenantRoot("Tenant1", "Company1", "admin1@example.com", Guid.NewGuid()),
            new TenantRoot("Tenant2", "Company2", "admin2@example.com", Guid.NewGuid())
        };
        TenantServiceMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenants);
        SubscriptionServiceMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SubscriptionRoot>());
        TenantProviderMock.Setup(x => x.GetMasterTenantId()).Returns(Guid.NewGuid());

        // Act
        var cut = RenderComponent<TenantManagementPage>();

        // Assert
        TenantServiceMock.Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.Contains("Tenant1", cut.Markup);
        Assert.Contains("Tenant2", cut.Markup);
    }

    [Fact]
    public async Task AddTenant_Should_Call_TenantService_AddAsync()
    {
        // Arrange
        TenantServiceMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TenantRoot>());
        SubscriptionServiceMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SubscriptionRoot>());
        
        var cut = RenderComponent<TenantManagementPage>();
        
        // Act
        await cut.InvokeAsync(() => cut.Instance.OpenCreateDialog());
        
        cut.Instance.Model.TenantName = "New Tenant";
        cut.Instance.Model.CompanyName = "New Company";
        cut.Instance.Model.AdminEmail = "admin@example.com";
        cut.Instance.Model.SubscriptionId = Guid.NewGuid();

        await cut.InvokeAsync(async () => await cut.Instance.SaveAsync());

        // Assert
        TenantServiceMock.Verify(x => x.AddAsync(It.IsAny<TenantRoot>(), It.IsAny<CancellationToken>()), Times.Once);
        SnackbarMock.Verify(x => x.Add(It.IsAny<string>(), Severity.Success, It.IsAny<Action<SnackbarOptions>>(), It.IsAny<string>()), Times.Once);
    }
}

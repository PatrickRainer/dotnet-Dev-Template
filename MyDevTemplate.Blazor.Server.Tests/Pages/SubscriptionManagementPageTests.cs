using Bunit;
using Moq;
using MudBlazor;
using MyDevTemplate.Blazor.Server.Components.Pages.Company.SubscriptionManagement;
using MyDevTemplate.Domain.Entities.SubscriptionAggregate;

namespace MyDevTemplate.Blazor.Server.Tests.Pages;

public class SubscriptionManagementPageTests : BlazorTestBase
{
    [Fact]
    public void SubscriptionManagementPage_Should_LoadSubscriptions_OnInitialized()
    {
        // Arrange
        var subscriptions = new List<SubscriptionRoot>
        {
            new SubscriptionRoot("Basic", "Basic plan"),
            new SubscriptionRoot("Pro", "Professional plan")
        };
        SubscriptionServiceMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscriptions);

        // Act
        var cut = RenderComponent<SubscriptionManagementPage>();

        // Assert
        SubscriptionServiceMock.Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.Contains("Basic", cut.Markup);
        Assert.Contains("Pro", cut.Markup);
    }

    [Fact]
    public async Task AddSubscription_Should_Call_SubscriptionService_AddAsync()
    {
        // Arrange
        SubscriptionServiceMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SubscriptionRoot>());
        
        var cut = RenderComponent<SubscriptionManagementPage>();
        
        // Act
        await cut.InvokeAsync(() => cut.Instance.OpenCreateDialog());
        
        cut.Instance.Model.Name = "New Subscription";
        cut.Instance.Model.Description = "New Description";
        cut.Instance.Model.SelectedFeatures = new List<string> { SubscriptionFeatures.Automation };

        await cut.InvokeAsync(async () => await cut.Instance.SaveAsync());

        // Assert
        SubscriptionServiceMock.Verify(x => x.AddAsync(It.IsAny<SubscriptionRoot>(), It.IsAny<CancellationToken>()), Times.Once);
        SnackbarMock.Verify(x => x.Add(It.IsAny<string>(), Severity.Success, It.IsAny<Action<SnackbarOptions>>(), It.IsAny<string>()), Times.Once);
    }
}

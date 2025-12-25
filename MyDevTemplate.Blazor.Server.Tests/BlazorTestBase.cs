using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor;
using MudBlazor.Services;
using MyDevTemplate.Application.SubscriptionServices;
using MyDevTemplate.Application.TenantServices;
using MyDevTemplate.Application.UserServices;
using MyDevTemplate.Domain.Contracts.Abstractions;

namespace MyDevTemplate.Blazor.Server.Tests;

public abstract class BlazorTestBase : TestContext
{
    protected Mock<IUserService> UserServiceMock { get; } = new();
    protected Mock<ISubscriptionService> SubscriptionServiceMock { get; } = new();
    protected Mock<ITenantService> TenantServiceMock { get; } = new();
    protected Mock<ITenantProvider> TenantProviderMock { get; } = new();
    protected Mock<ISnackbar> SnackbarMock { get; } = new();

    protected BlazorTestBase()
    {
        Services.AddMudServices();
        Services.AddSingleton(UserServiceMock.Object);
        Services.AddSingleton(SubscriptionServiceMock.Object);
        Services.AddSingleton(TenantServiceMock.Object);
        Services.AddSingleton(TenantProviderMock.Object);
        Services.AddSingleton(SnackbarMock.Object);

        // Required for MudBlazor components that use JS Interop
        JSInterop.Mode = JSRuntimeMode.Loose;
    }
}

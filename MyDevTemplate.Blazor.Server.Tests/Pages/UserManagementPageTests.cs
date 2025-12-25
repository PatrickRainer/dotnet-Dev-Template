using Bunit;
using Microsoft.AspNetCore.Components.Web;
using Moq;
using MudBlazor;
using MyDevTemplate.Blazor.Server.Components.Pages.Company.UserManagement;
using MyDevTemplate.Domain.Entities.Common;
using MyDevTemplate.Domain.Entities.UserAggregate;

namespace MyDevTemplate.Blazor.Server.Tests.Pages;

public class UserManagementPageTests : BlazorTestBase
{
    [Fact]
    public void UserManagementPage_Should_LoadUsers_OnInitialized()
    {
        // Arrange
        var users = new List<UserRoot>
        {
            new UserRoot(new EmailAddress("test1@example.com"), "John", "Doe", "id1"),
            new UserRoot(new EmailAddress("test2@example.com"), "Jane", "Smith", "id2")
        };
        UserServiceMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        // Act
        var cut = RenderComponent<UserManagementPage>();

        // Assert
        UserServiceMock.Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        var rows = cut.FindAll("tr");
        // MudTable has a header row and data rows. 
        // We expect at least 2 data rows if rendered correctly.
        // Depending on MudTable implementation it might be different, let's check for text.
        Assert.Contains("John", cut.Markup);
        Assert.Contains("Doe", cut.Markup);
        Assert.Contains("Jane", cut.Markup);
        Assert.Contains("Smith", cut.Markup);
    }

    [Fact]
    public async Task AddUser_Should_Call_UserService_AddAsync()
    {
        // Arrange
        UserServiceMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserRoot>());
        
        var cut = RenderComponent<UserManagementPage>();
        
        // Act - Open dialog
        var addButton = cut.Find("button.mud-button-filled-primary");
        await addButton.ClickAsync(new MouseEventArgs());
        
        // Directly set the model and call Submit to test the logic
        // (Simulating the form filling)
        cut.Instance.Model = new UserManagementModel 
        { 
            Email = "new@example.com", 
            FirstName = "New", 
            LastName = "User" 
        };
        
        // Mock the form validation to return true
        // In a real integration test, we might want to use the form itself, 
        // but MudForm is complex to mock/use in bUnit without a full render.
        // For now, let's bypass the form check by setting it if possible, 
        // or just calling the logic if we made it accessible.
        
        // Since we want to test the page functionality, let's try to make it work with MudForm.
        // Actually, let's just make Submit public and call it.
        
        await cut.InvokeAsync(async () => await cut.Instance.SaveAsync());

        // Assert
        UserServiceMock.Verify(x => x.AddAsync(It.IsAny<UserRoot>(), It.IsAny<CancellationToken>()), Times.Once);
        SnackbarMock.Verify(x => x.Add(It.IsAny<string>(), Severity.Success, It.IsAny<Action<SnackbarOptions>>(), It.IsAny<string>()), Times.Once);
    }
}

using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using MyDevTemplate.Application.UserServices;
using MyDevTemplate.Domain.Entities.Common;
using MyDevTemplate.Domain.Entities.UserAggregate;

namespace MyDevTemplate.Api.IntegrationTests;

public class UserServiceTests : IntegrationTestBase
{
    public UserServiceTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    private void SetTenantContext(IServiceProvider serviceProvider, string tenantId, string userName = "TestUser")
    {
        var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
        var context = new DefaultHttpContext();
        var claims = new List<Claim>
        {
            new Claim("TenantId", tenantId),
            new Claim(ClaimTypes.Name, userName)
        };
        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        httpContextAccessor.HttpContext = context;
    }

    [Fact]
    public async Task AddUserAsync_ShouldAddUserToDatabase()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        SetTenantContext(scope.ServiceProvider, TenantId);
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

        var email = $"add.test.{Guid.NewGuid()}@example.com";
        var user = new UserRoot(new EmailAddress(email), "First", "Last", "oid-1");

        // Act
        await userService.AddAsync(user);

        // Assert
        var retrievedUser = await userService.GetUserByEmailAsync(email);
        Assert.NotNull(retrievedUser);
        Assert.Equal(email, retrievedUser.Email.Value);
        Assert.Equal("First", retrievedUser.FirstName);
        Assert.Equal(Guid.Parse(TenantId), retrievedUser.TenantId);

        // Cleanup
        await userService.RemoveUserByEmailAsync(email);
    }

    [Fact]
    public async Task GetUserByEmailAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        SetTenantContext(scope.ServiceProvider, TenantId);
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

        // Act
        var user = await userService.GetUserByEmailAsync($"nonexistent.{Guid.NewGuid()}@example.com");

        // Assert
        Assert.Null(user);
    }

    [Fact]
    public async Task RemoveUserAsync_ShouldThrowException_WhenUserDoesNotExist()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        SetTenantContext(scope.ServiceProvider, TenantId);
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => userService.RemoveUserByEmailAsync($"nonexistent.{Guid.NewGuid()}@example.com"));
    }

    [Fact]
    public async Task UpsertUserFromEntraAsync_ShouldCreateNewUser_WhenUserDoesNotExist()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        SetTenantContext(scope.ServiceProvider, TenantId);
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

        var email = $"upsert.new.{Guid.NewGuid()}@example.com";
        var oid = "oid-new";

        // Act
        await userService.UpsertAfterLogin(oid, email);

        // Assert
        var user = await userService.GetUserByEmailAsync(email);
        Assert.NotNull(user);
        Assert.Equal(oid, user.IdentityProviderId);

        // Cleanup
        await userService.RemoveUserByEmailAsync(email);
    }

    [Fact]
    public async Task UpsertAfterLogin_ShouldMapToExistingUser_WhenIdentityProviderIdIsEmpty()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        SetTenantContext(scope.ServiceProvider, TenantId);
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

        var email = $"map.test.{Guid.NewGuid()}@example.com";
        // Create user with empty IdentityProviderId (as if added by admin)
        var user = new UserRoot(new EmailAddress(email), "First", "Last", "");
        await userService.AddAsync(user);

        // Act - Simulate login which calls UpsertAfterLogin
        var oidFromEntra = "oid-from-entra";
        await userService.UpsertAfterLogin(oidFromEntra, email);

        // Assert
        var updatedUser = await userService.GetUserByEmailAsync(email);
        Assert.NotNull(updatedUser);
        Assert.Equal(oidFromEntra, updatedUser.IdentityProviderId);
        Assert.NotNull(updatedUser.LastLoginAtUtc);

        // Cleanup
        await userService.RemoveUserByEmailAsync(email);
    }

    [Fact]
    public async Task UpsertAfterLogin_ShouldMapToExistingUser_EvenIfTenantContextIsMissing()
    {
        // Arrange
        using var scope1 = Factory.Services.CreateScope();
        SetTenantContext(scope1.ServiceProvider, TenantId);
        var userService1 = scope1.ServiceProvider.GetRequiredService<IUserService>();

        var email = $"map.cross.{Guid.NewGuid()}@example.com";
        var user = new UserRoot(new EmailAddress(email), "First", "Last", "");
        await userService1.AddAsync(user);

        // Act - Simulate login WITHOUT tenant context
        using var scope2 = Factory.Services.CreateScope();
        // NOT calling SetTenantContext here
        var userService2 = scope2.ServiceProvider.GetRequiredService<IUserService>();
        
        var oidFromEntra = "oid-cross";
        await userService2.UpsertAfterLogin(oidFromEntra, email);

        // Assert
        SetTenantContext(scope2.ServiceProvider, TenantId); // Set it back to retrieve the user
        var updatedUser = await userService2.GetUserByEmailAsync(email);
        Assert.NotNull(updatedUser);
        Assert.Equal(oidFromEntra, updatedUser.IdentityProviderId);

        // Cleanup
        await userService1.RemoveUserByEmailAsync(email);
    }

    [Fact]
    public async Task UpsertUserFromEntraAsync_ShouldUpdateOid_WhenUserExistsWithDifferentOid()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        SetTenantContext(scope.ServiceProvider, TenantId);
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

        var email = $"upsert.update.{Guid.NewGuid()}@example.com";
        var initialOid = "oid-initial";
        var updatedOid = "oid-updated";

        var user = new UserRoot(new EmailAddress(email), "First", "Last", initialOid);
        await userService.AddAsync(user);

        // Act
        await userService.UpsertAfterLogin(updatedOid, email);

        // Assert
        var updatedUser = await userService.GetUserByEmailAsync(email);
        Assert.NotNull(updatedUser);
        Assert.Equal(updatedOid, updatedUser.IdentityProviderId);

        // Cleanup
        await userService.RemoveUserByEmailAsync(email);
    }

    [Fact]
    public async Task UpsertUserFromEntraAsync_ShouldThrowException_WhenEmailIsInvalid()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        SetTenantContext(scope.ServiceProvider, TenantId);
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => userService.UpsertAfterLogin("oid", null!));
        await Assert.ThrowsAsync<ArgumentException>(() => userService.UpsertAfterLogin("oid", ""));
        await Assert.ThrowsAsync<ArgumentException>(() => userService.UpsertAfterLogin("oid", "   "));
        await Assert.ThrowsAsync<ArgumentException>(() => userService.UpsertAfterLogin("oid", "invalid-email"));
    }

    [Fact]
    public async Task GetUserByEmailAsync_ShouldThrowException_WhenEmailIsInvalid()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        SetTenantContext(scope.ServiceProvider, TenantId);
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => userService.GetUserByEmailAsync("invalid-email"));
    }

    [Fact]
    public async Task AddFeatureToUserAsync_ShouldThrowException_WhenFeatureIsNotSubscribed()
    {
        // Arrange
        Guid userId;
        string email;
        using (var scope = Factory.Services.CreateScope())
        {
            SetTenantContext(scope.ServiceProvider, TenantId);
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
            email = $"feature.test.{Guid.NewGuid()}@example.com";
            var user = new UserRoot(new EmailAddress(email), "First", "Last", "oid-f1");
            userId = await userService.AddAsync(user);
        }

        var unsubscribedFeature = "NonExistentFeature_" + Guid.NewGuid();

        // Act & Assert
        using (var scope = Factory.Services.CreateScope())
        {
            SetTenantContext(scope.ServiceProvider, TenantId);
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                userService.AddFeatureToUserAsync(userId, unsubscribedFeature));
        }

        // Cleanup
        using (var scope = Factory.Services.CreateScope())
        {
            SetTenantContext(scope.ServiceProvider, TenantId);
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
            await userService.RemoveUserByEmailAsync(email);
        }
    }

    [Fact]
    public async Task AddFeatureToUserAsync_ShouldSucceed_WhenMasterTenant()
    {
        // Arrange
        Guid userId;
        string email;
        using (var scope = Factory.Services.CreateScope())
        {
            SetTenantContext(scope.ServiceProvider, TenantId, "MasterKeyUser");
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
            email = $"feature.master.{Guid.NewGuid()}@example.com";
            var user = new UserRoot(new EmailAddress(email), "First", "Last", "oid-f2");
            userId = await userService.AddAsync(user);
        }

        var feature = "AnyFeature_" + Guid.NewGuid(); 

        // Act
        using (var scope = Factory.Services.CreateScope())
        {
            SetTenantContext(scope.ServiceProvider, TenantId, "MasterKeyUser");
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
            await userService.AddFeatureToUserAsync(userId, feature);

            // Assert
            var updatedUser = await userService.GetByIdAsync(userId);
            Assert.Contains(feature, updatedUser!.AllowedFeatures);
        }

        // Cleanup
        using (var scope = Factory.Services.CreateScope())
        {
            SetTenantContext(scope.ServiceProvider, TenantId, "MasterKeyUser");
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
            await userService.RemoveUserByEmailAsync(email);
        }
    }
}

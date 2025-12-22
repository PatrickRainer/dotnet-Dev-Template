using MyDevTemplate.Domain.Entities.ApiKeyAggregate;
using Xunit;

namespace MyDevTemplate.Domain.Tests;

public class ApiKeyRootEntityTests
{
    [Fact]
    public void ApiKeyRootEntity_Should_Initialize_Correctly()
    {
        // Arrange
        var key = "test-key-123";
        var clientId = "test-client-id";
        var label = "Test Key";
        var expiresAtUtc = DateTime.UtcNow.AddDays(7);

        // Act
        var apiKey = new ApiKeyRootEntity(key, clientId, label, expiresAtUtc);

        // Assert
        Assert.Equal(key, apiKey.Key);
        Assert.Equal(clientId, apiKey.ClientId);
        Assert.Equal(label, apiKey.Label);
        Assert.Equal(expiresAtUtc, apiKey.ExpiresAtUtc);
        Assert.True(apiKey.IsActive);
        Assert.True(apiKey.IsValid);
        Assert.False(apiKey.IsExpired);
    }

    [Fact]
    public void Deactivate_Should_Make_ApiKey_Inactive()
    {
        // Arrange
        var apiKey = new ApiKeyRootEntity("key", "clientId", "label");

        // Act
        apiKey.Deactivate();

        // Assert
        Assert.False(apiKey.IsActive);
        Assert.False(apiKey.IsValid);
    }

    [Fact]
    public void Activate_Should_Make_ApiKey_Active()
    {
        // Arrange
        var apiKey = new ApiKeyRootEntity("key", "clientId", "label");
        apiKey.Deactivate();

        // Act
        apiKey.Activate();

        // Assert
        Assert.True(apiKey.IsActive);
        Assert.True(apiKey.IsValid);
    }

    [Fact]
    public void IsExpired_Should_Be_True_When_Expired()
    {
        // Arrange
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(-1);
        var apiKey = new ApiKeyRootEntity("key", "clientId", "label", expiresAtUtc);

        // Act & Assert
        Assert.True(apiKey.IsExpired);
        Assert.False(apiKey.IsValid);
    }

    [Theory]
    [InlineData("", "clientId", "label")]
    [InlineData("key", "", "label")]
    [InlineData("key", "clientId", "")]
    [InlineData(null, "clientId", "label")]
    [InlineData("key", null, "label")]
    [InlineData("key", "clientId", null)]
    public void Constructor_Should_Throw_ArgumentException_For_Invalid_Inputs(string key, string clientId, string label)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new ApiKeyRootEntity(key, clientId, label));
    }
}

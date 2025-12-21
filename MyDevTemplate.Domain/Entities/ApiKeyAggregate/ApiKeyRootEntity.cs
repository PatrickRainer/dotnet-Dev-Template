using MyDevTemplate.Domain.Entities.Abstractions;

namespace MyDevTemplate.Domain.Entities.ApiKeyAggregate;

public class ApiKeyRootEntity : EntityBase
{
    // For EF Core
    private ApiKeyRootEntity()
    {
        Key = string.Empty;
        Label = string.Empty;
    }

    public ApiKeyRootEntity(string key, string label, DateTime? expiresAtUtc = null)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be null or whitespace", nameof(key));
        
        if (string.IsNullOrWhiteSpace(label))
            throw new ArgumentException("Label cannot be null or whitespace", nameof(label));

        Key = key;
        Label = label;
        ExpiresAtUtc = expiresAtUtc;
        IsActive = true;
    }

    public string Key { get; private set; }
    public string Label { get; set; }
    public bool IsActive { get; set; }
    public DateTime? ExpiresAtUtc { get; set; }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public bool IsExpired => ExpiresAtUtc.HasValue && ExpiresAtUtc.Value < DateTime.UtcNow;
    public bool IsValid => IsActive && !IsExpired;
}

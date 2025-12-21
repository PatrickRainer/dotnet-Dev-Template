namespace MyDevTemplate.Domain.Entities.Abstractions;

public abstract class EntityBase
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public Guid TenantId { get; set; }
}
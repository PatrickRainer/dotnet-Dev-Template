using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyDevTemplate.Domain.Entities.ApiKeyAggregate;

namespace MyDevTemplate.Persistence.ModelConfigurations;

public class ApiKeyConfiguration : IEntityTypeConfiguration<ApiKeyRootEntity>
{
    public void Configure(EntityTypeBuilder<ApiKeyRootEntity> builder)
    {
        builder.ToTable("ApiKeys");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .ValueGeneratedNever();

        builder.Property(a => a.Key)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(a => a.ClientId)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(a => a.Key).IsUnique();
        builder.HasIndex(a => a.ClientId).IsUnique();

        builder.Property(a => a.Label)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.IsActive)
            .IsRequired();

        builder.Property(a => a.ExpiresAtUtc)
            .IsRequired(false);

        builder.Property(a => a.CreatedAtUtc)
            .IsRequired();

        builder.Property(a => a.TenantId)
            .IsRequired();
    }
}

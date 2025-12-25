using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyDevTemplate.Domain.Entities.RoleAggregate;

namespace MyDevTemplate.Persistence.ModelConfigurations;

public class RoleConfiguration : IEntityTypeConfiguration<RoleRoot>
{
    public void Configure(EntityTypeBuilder<RoleRoot> builder)
    {
        builder.ToTable("Roles");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .ValueGeneratedNever();

        builder.Property(r => r.Title)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(r => r.Description)
            .HasMaxLength(1000);

        builder.Property(r => r.CreatedAtUtc)
            .IsRequired();

        builder.Property(r => r.TenantId)
            .IsRequired();

        builder.Property(r => r.Features)
            .HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList())
            .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c.ToList()));

        builder.Property(r => r.Users)
            .HasField("_users")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(Guid.Parse).ToList())
            .Metadata.SetValueComparer(new ValueComparer<IReadOnlyCollection<Guid>>(
                (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => (IReadOnlyCollection<Guid>)c.ToList()));
    }
}

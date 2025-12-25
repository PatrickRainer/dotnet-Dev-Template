using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyDevTemplate.Domain.Entities.SubscriptionAggregate;

namespace MyDevTemplate.Persistence.ModelConfigurations;

public class SubscriptionConfiguration : IEntityTypeConfiguration<SubscriptionRoot>
{
    public void Configure(EntityTypeBuilder<SubscriptionRoot> builder)
    {
        builder.ToTable("Subscriptions");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .ValueGeneratedNever();

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(s => s.Description)
            .HasMaxLength(1000);

        builder.Property(s => s.CreatedAtUtc)
            .IsRequired();

        builder.Property(s => s.TenantId)
            .IsRequired();

        builder.Property(s => s.Features)
            .HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList())
            .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c.ToList()));

        builder.HasData(
            new
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                Name = "Subscription 1",
                Description = "Basic business management platform.",
                Features = new List<string> { SubscriptionFeatures.Dashboard, SubscriptionFeatures.Analytics, SubscriptionFeatures.Settings },
                CreatedAtUtc = DateTime.Parse("2024-01-01T00:00:00Z"),
                TenantId = Guid.Empty
            },
            new
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                Name = "Subscription 2",
                Description = "Enhanced business management and collaboration.",
                Features = new List<string> { SubscriptionFeatures.Dashboard, SubscriptionFeatures.Analytics, SubscriptionFeatures.Settings, SubscriptionFeatures.Reports, SubscriptionFeatures.Notifications, SubscriptionFeatures.UserManagement, SubscriptionFeatures.Integration },
                CreatedAtUtc = DateTime.Parse("2024-01-01T00:00:00Z"),
                TenantId = Guid.Empty
            },
            new
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000003"),
                Name = "Subscription 3",
                Description = "Enterprise-level business management and automation.",
                Features = new List<string> { SubscriptionFeatures.Dashboard, SubscriptionFeatures.Analytics, SubscriptionFeatures.Settings, SubscriptionFeatures.Reports, SubscriptionFeatures.Notifications, SubscriptionFeatures.UserManagement, SubscriptionFeatures.Integration, SubscriptionFeatures.AdvancedAnalytics, SubscriptionFeatures.Automation, SubscriptionFeatures.Security, SubscriptionFeatures.ApiAccess, SubscriptionFeatures.CustomWorkflows },
                CreatedAtUtc = DateTime.Parse("2024-01-01T00:00:00Z"),
                TenantId = Guid.Empty
            }
        );
    }
}

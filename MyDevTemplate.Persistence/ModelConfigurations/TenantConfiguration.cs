using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyDevTemplate.Domain.Entities.TenantAggregate;

namespace MyDevTemplate.Persistence.ModelConfigurations;

public class TenantConfiguration : IEntityTypeConfiguration<TenantRoot>
{
    public void Configure(EntityTypeBuilder<TenantRoot> builder)
    {
        builder.ToTable("Tenants");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .ValueGeneratedNever();

        builder.Property(t => t.TenantName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.CompanyName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.SubscriptionId);
        
        builder.Property(t => t.AdminEmail)
            .IsRequired()
            .HasMaxLength(256);

        builder.OwnsOne(t => t.Address, address =>
        {
            address.Property(a => a.Street)
                .HasColumnName("Street")
                .HasMaxLength(200);
            
            address.Property(a => a.City)
                .HasColumnName("City")
                .HasMaxLength(100);
            
            address.Property(a => a.State)
                .HasColumnName("State")
                .HasMaxLength(100);
            
            address.Property(a => a.Country)
                .HasColumnName("Country")
                .HasMaxLength(100);
            
            address.Property(a => a.ZipCode)
                .HasColumnName("ZipCode")
                .HasMaxLength(20);
        });

        builder.Property(t => t.CreatedAtUtc)
            .IsRequired();

        builder.Property(t => t.TenantId)// Todo why it has this property and not using ID?
            .IsRequired();
    }
}

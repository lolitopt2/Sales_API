using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SalesManagement.Domain.Entities;

namespace SalesManagement.Infrastructure.Data.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(c => c.Email)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(c => c.Email)
            .IsUnique();

        builder.Property(c => c.Phone)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(c => c.DocumentNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(c => c.DocumentNumber)
            .IsUnique();

        builder.Property(c => c.Address)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(c => c.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(c => c.CreatedAt)
            .IsRequired();
    }
}

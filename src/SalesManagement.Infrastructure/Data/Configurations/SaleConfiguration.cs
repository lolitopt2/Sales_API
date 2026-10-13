using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SalesManagement.Domain.Entities;

namespace SalesManagement.Infrastructure.Data.Configurations;

public class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.ToTable("Sales");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.SaleNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(s => s.SaleNumber)
            .IsUnique();

        builder.Property(s => s.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(s => s.TotalAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(s => s.SaleDate)
            .IsRequired();

        builder.Property(s => s.CreatedAt)
            .IsRequired();

        builder.HasOne(s => s.Customer)
            .WithMany(c => c.Sales)
            .HasForeignKey(s => s.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.User)
            .WithMany(u => u.Sales)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(s => s.Items)
            .WithOne(i => i.Sale)
            .HasForeignKey(i => i.SaleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

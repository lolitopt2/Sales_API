using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SalesManagement.Domain.Entities;

namespace SalesManagement.Infrastructure.Data.Configurations;

public class SaleItemConfiguration : IEntityTypeConfiguration<SaleItem>
{
    public void Configure(EntityTypeBuilder<SaleItem> builder)
    {
        builder.ToTable("SaleItems");

        builder.HasKey(si => si.Id);

        builder.Property(si => si.Quantity)
            .IsRequired();

        builder.Property(si => si.UnitPrice)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(si => si.CreatedAt)
            .IsRequired();

        builder.HasOne(si => si.Product)
            .WithMany()
            .HasForeignKey(si => si.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

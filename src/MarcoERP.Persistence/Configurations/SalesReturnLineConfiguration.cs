using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MarcoERP.Domain.Entities.Inventory;
using MarcoERP.Domain.Entities.Sales;

namespace MarcoERP.Persistence.Configurations
{
    public sealed class SalesReturnLineConfiguration : IEntityTypeConfiguration<SalesReturnLine>
    {
        public void Configure(EntityTypeBuilder<SalesReturnLine> builder)
        {
            builder.ToTable("SalesReturnLines");

            builder.HasKey(l => l.Id);
            builder.Property(l => l.Id).UseIdentityColumn();

            builder.Property(l => l.RowVersion).IsRowVersion().IsConcurrencyToken();

            builder.Property(l => l.SalesReturnId).IsRequired();
            builder.Property(l => l.ProductId).IsRequired();
            builder.Property(l => l.UnitId).IsRequired();
            builder.Property(l => l.Quantity).IsRequired().HasPrecision(18, 4);
            builder.Property(l => l.UnitPrice).IsRequired().HasPrecision(18, 4);
            builder.Property(l => l.ConversionFactor).IsRequired().HasPrecision(18, 6);
            builder.Property(l => l.BaseQuantity).IsRequired().HasPrecision(18, 4);
            builder.Property(l => l.DiscountPercent).IsRequired().HasPrecision(5, 2);
            builder.Property(l => l.DiscountAmount).IsRequired().HasPrecision(18, 4);
            builder.Property(l => l.SubTotal).IsRequired().HasPrecision(18, 4);
            builder.Property(l => l.NetTotal).IsRequired().HasPrecision(18, 4);
            builder.Property(l => l.VatRate).IsRequired().HasPrecision(5, 2);
            builder.Property(l => l.VatAmount).IsRequired().HasPrecision(18, 4);
            builder.Property(l => l.TotalWithVat).IsRequired().HasPrecision(18, 4);

            // Relationships
            builder.HasOne<Product>().WithMany()
                .HasForeignKey(l => l.ProductId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<Unit>().WithMany()
                .HasForeignKey(l => l.UnitId).OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(l => l.SalesReturnId)
                .HasDatabaseName("IX_SalesReturnLines_ReturnId");
            builder.HasIndex(l => l.ProductId)
                .HasDatabaseName("IX_SalesReturnLines_ProductId");
        }
    }
}

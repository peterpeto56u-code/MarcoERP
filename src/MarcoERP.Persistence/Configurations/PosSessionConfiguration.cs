using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MarcoERP.Domain.Entities.Sales;
using MarcoERP.Domain.Entities.Treasury;
using MarcoERP.Domain.Entities.Inventory;
using MarcoERP.Domain.Entities.Security;

namespace MarcoERP.Persistence.Configurations
{
    public sealed class PosSessionConfiguration : IEntityTypeConfiguration<PosSession>
    {
        public void Configure(EntityTypeBuilder<PosSession> builder)
        {
            builder.ToTable("PosSessions");

            builder.HasKey(ps => ps.Id);
            builder.Property(ps => ps.Id).UseIdentityColumn();

            builder.Property(ps => ps.RowVersion).IsRowVersion().IsConcurrencyToken();

            builder.Property(ps => ps.SessionNumber).IsRequired().HasMaxLength(30).IsUnicode(false);
            builder.Property(ps => ps.Status).IsRequired().HasConversion<int>();
            builder.Property(ps => ps.OpeningBalance).IsRequired().HasPrecision(18, 4);
            builder.Property(ps => ps.TotalSales).IsRequired().HasPrecision(18, 4);
            builder.Property(ps => ps.TotalCashReceived).IsRequired().HasPrecision(18, 4);
            builder.Property(ps => ps.TotalCardReceived).IsRequired().HasPrecision(18, 4);
            builder.Property(ps => ps.TotalOnAccount).IsRequired().HasPrecision(18, 4);
            builder.Property(ps => ps.TransactionCount).IsRequired();
            builder.Property(ps => ps.ClosingBalance).IsRequired().HasPrecision(18, 4);
            builder.Property(ps => ps.Variance).IsRequired().HasPrecision(18, 4);
            builder.Property(ps => ps.OpenedAt).IsRequired();
            builder.Property(ps => ps.ClosingNotes).HasMaxLength(1000);

            // Audit
            builder.Property(ps => ps.CreatedAt).IsRequired();
            builder.Property(ps => ps.CreatedBy).IsRequired().HasMaxLength(100);
            builder.Property(ps => ps.ModifiedBy).HasMaxLength(100);

            // Relationships
            builder.HasOne<User>().WithMany()
                .HasForeignKey(ps => ps.UserId).OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Cashbox>().WithMany()
                .HasForeignKey(ps => ps.CashboxId).OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Warehouse>().WithMany()
                .HasForeignKey(ps => ps.WarehouseId).OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(ps => ps.Payments).WithOne(p => p.PosSession)
                .HasForeignKey(p => p.PosSessionId).OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(ps => ps.SessionNumber).IsUnique()
                .HasDatabaseName("IX_PosSessions_SessionNumber");
            builder.HasIndex(ps => ps.UserId)
                .HasDatabaseName("IX_PosSessions_UserId");
            builder.HasIndex(ps => ps.Status)
                .HasDatabaseName("IX_PosSessions_Status");
            builder.HasIndex(ps => ps.OpenedAt)
                .HasDatabaseName("IX_PosSessions_OpenedAt");
        }
    }
}

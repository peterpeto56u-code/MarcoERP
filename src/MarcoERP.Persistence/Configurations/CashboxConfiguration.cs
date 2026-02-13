using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MarcoERP.Domain.Entities.Treasury;

namespace MarcoERP.Persistence.Configurations
{
    public sealed class CashboxConfiguration : IEntityTypeConfiguration<Cashbox>
    {
        public void Configure(EntityTypeBuilder<Cashbox> builder)
        {
            builder.ToTable("Cashboxes");

            builder.HasKey(c => c.Id);
            builder.Property(c => c.Id).UseIdentityColumn();

            builder.Property(c => c.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            builder.Property(c => c.Code)
                .IsRequired()
                .HasMaxLength(10);

            builder.Property(c => c.NameAr)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.NameEn)
                .HasMaxLength(100);

            builder.Property(c => c.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(c => c.IsDefault)
                .IsRequired()
                .HasDefaultValue(false);

            // ── Audit Fields ────────────────────────────────────
            builder.Property(c => c.CreatedAt).IsRequired();
            builder.Property(c => c.CreatedBy).IsRequired().HasMaxLength(100);
            builder.Property(c => c.ModifiedBy).HasMaxLength(100);

            // ── Soft Delete Fields ──────────────────────────────
            builder.Property(c => c.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(c => c.DeletedAt);

            builder.Property(c => c.DeletedBy)
                .HasMaxLength(100);

            // ── Optional relationship to GL Account ─────────────
            builder.Property(c => c.AccountId);

            // ── Indexes ─────────────────────────────────────────
            builder.HasIndex(c => c.Code)
                .IsUnique()
                .HasDatabaseName("IX_Cashboxes_Code");
        }
    }
}

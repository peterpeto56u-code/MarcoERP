using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MarcoERP.Domain.Entities.Security;

namespace MarcoERP.Persistence.Configurations
{
    public sealed class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
    {
        public void Configure(EntityTypeBuilder<RolePermission> builder)
        {
            builder.ToTable("RolePermissions");

            builder.HasKey(rp => rp.Id);
            builder.Property(rp => rp.Id).UseIdentityColumn();

            builder.Property(rp => rp.PermissionKey)
                .IsRequired()
                .HasMaxLength(100);

            // ── Relationships ───────────────────────────────────
            builder.HasOne(rp => rp.Role)
                .WithMany(r => r.Permissions)
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // ── Indexes ─────────────────────────────────────────
            builder.HasIndex(rp => new { rp.RoleId, rp.PermissionKey })
                .IsUnique()
                .HasDatabaseName("IX_RolePermissions_RoleId_PermissionKey");
        }
    }
}

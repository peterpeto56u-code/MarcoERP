using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MarcoERP.Domain.Entities.Accounting;
using MarcoERP.Domain.Entities.Common;
using MarcoERP.Domain.Entities.Inventory;
using MarcoERP.Domain.Entities.Sales;
using MarcoERP.Domain.Entities.Purchases;
using MarcoERP.Domain.Entities.Treasury;
using MarcoERP.Domain.Entities.Security;
using MarcoERP.Domain.Entities.Settings;

namespace MarcoERP.Persistence
{
    /// <summary>
    /// Central EF Core DbContext for MarcoERP.
    /// Configures all entity mappings via Fluent API (no Data Annotations).
    /// Applies global query filters for soft-deleted entities.
    /// </summary>
    public class MarcoDbContext : DbContext
    {
        public MarcoDbContext(DbContextOptions<MarcoDbContext> options)
            : base(options)
        {
        }

        // ── DbSets ──────────────────────────────────────────────

        // ── Company DbSet ───────────────────────────────────────
        public DbSet<Company> Companies { get; set; }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<JournalEntry> JournalEntries { get; set; }
        public DbSet<JournalEntryLine> JournalEntryLines { get; set; }
        public DbSet<FiscalYear> FiscalYears { get; set; }
        public DbSet<FiscalPeriod> FiscalPeriods { get; set; }
        public DbSet<CodeSequence> CodeSequences { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        // ── Inventory DbSets ────────────────────────────────────
        public DbSet<Category> Categories { get; set; }
        public DbSet<Unit> Units { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductUnit> ProductUnits { get; set; }
        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<WarehouseProduct> WarehouseProducts { get; set; }
        public DbSet<InventoryMovement> InventoryMovements { get; set; }

        // ── Inventory Adjustment DbSets ──────────────────────────
        public DbSet<InventoryAdjustment> InventoryAdjustments { get; set; }
        public DbSet<InventoryAdjustmentLine> InventoryAdjustmentLines { get; set; }

        // ── Sales & Purchases DbSets ────────────────────────────
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<SalesRepresentative> SalesRepresentatives { get; set; }

        // ── Price List DbSets ───────────────────────────────────
        public DbSet<PriceList> PriceLists { get; set; }
        public DbSet<PriceTier> PriceTiers { get; set; }

        // ── Purchase Document DbSets ────────────────────────────
        public DbSet<PurchaseInvoice> PurchaseInvoices { get; set; }
        public DbSet<PurchaseInvoiceLine> PurchaseInvoiceLines { get; set; }
        public DbSet<PurchaseReturn> PurchaseReturns { get; set; }
        public DbSet<PurchaseReturnLine> PurchaseReturnLines { get; set; }

        // ── Purchase Quotation DbSets ───────────────────────────
        public DbSet<PurchaseQuotation> PurchaseQuotations { get; set; }
        public DbSet<PurchaseQuotationLine> PurchaseQuotationLines { get; set; }

        // ── Sales Document DbSets ───────────────────────────────
        public DbSet<SalesInvoice> SalesInvoices { get; set; }
        public DbSet<SalesInvoiceLine> SalesInvoiceLines { get; set; }
        public DbSet<SalesReturn> SalesReturns { get; set; }
        public DbSet<SalesReturnLine> SalesReturnLines { get; set; }

        // ── Sales Quotation DbSets ──────────────────────────────
        public DbSet<SalesQuotation> SalesQuotations { get; set; }
        public DbSet<SalesQuotationLine> SalesQuotationLines { get; set; }

        // ── POS DbSets ──────────────────────────────────────────
        public DbSet<PosSession> PosSessions { get; set; }
        public DbSet<PosPayment> PosPayments { get; set; }

        // ── Treasury DbSets ─────────────────────────────────────
        public DbSet<Cashbox> Cashboxes { get; set; }
        public DbSet<BankAccount> BankAccounts { get; set; }
        public DbSet<BankReconciliation> BankReconciliations { get; set; }
        public DbSet<BankReconciliationItem> BankReconciliationItems { get; set; }
        public DbSet<CashReceipt> CashReceipts { get; set; }
        public DbSet<CashPayment> CashPayments { get; set; }
        public DbSet<CashTransfer> CashTransfers { get; set; }

        // ── Security DbSets ─────────────────────────────────────
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }

        // ── Settings DbSets ─────────────────────────────────────
        public DbSet<SystemSetting> SystemSettings { get; set; }
        public DbSet<BackupHistory> BackupHistory { get; set; }
        // ── Feature Governance DbSets ───────────────────────────
        public DbSet<Feature> Features { get; set; }
        public DbSet<FeatureChangeLog> FeatureChangeLogs { get; set; }

        // ── Profile DbSets ──────────────────────────────────────
        public DbSet<SystemProfile> SystemProfiles { get; set; }
        public DbSet<ProfileFeature> ProfileFeatures { get; set; }

        // ── Version & Integrity DbSets (Phase 5) ────────────────
        public DbSet<SystemVersion> SystemVersions { get; set; }
        public DbSet<FeatureVersion> FeatureVersions { get; set; }

        // ── Migration Engine DbSets (Phase 6) ───────────────────
        public DbSet<MigrationExecution> MigrationExecutions { get; set; }

        // ── Model Configuration ─────────────────────────────────

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply all IEntityTypeConfiguration<T> from this assembly
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(MarcoDbContext).Assembly);

            // ── CompanyId Convention for CompanyAwareEntity ──────
            // All entities inheriting CompanyAwareEntity get CompanyId with default=1 and FK to Companies
            foreach (var entityType in modelBuilder.Model.GetEntityTypes()
                .Where(et => typeof(CompanyAwareEntity).IsAssignableFrom(et.ClrType)
                             && !et.ClrType.IsAbstract))
            {
                var entityBuilder = modelBuilder.Entity(entityType.ClrType);

                entityBuilder.Property(nameof(CompanyAwareEntity.CompanyId))
                    .IsRequired()
                    .HasDefaultValue(1);

                entityBuilder.HasOne(typeof(Company))
                    .WithMany()
                    .HasForeignKey(nameof(CompanyAwareEntity.CompanyId))
                    .OnDelete(DeleteBehavior.Restrict);

                entityBuilder.HasIndex(nameof(CompanyAwareEntity.CompanyId));
            }

            // ── Global Query Filters for Soft Delete ────────────
            // All entities inheriting SoftDeletableEntity automatically filtered
            foreach (var entityType in modelBuilder.Model.GetEntityTypes()
                .Where(entityType => typeof(SoftDeletableEntity).IsAssignableFrom(entityType.ClrType)))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .HasQueryFilter(BuildSoftDeleteFilter(entityType.ClrType));
            }
        }

        /// <summary>
        /// Builds a lambda expression: entity => !((SoftDeletableEntity)entity).IsDeleted
        /// </summary>
        private static System.Linq.Expressions.LambdaExpression BuildSoftDeleteFilter(Type entityType)
        {
            var parameter = System.Linq.Expressions.Expression.Parameter(entityType, "e");
            var isDeletedProperty = System.Linq.Expressions.Expression.Property(parameter, nameof(SoftDeletableEntity.IsDeleted));
            var notDeleted = System.Linq.Expressions.Expression.Not(isDeletedProperty);
            return System.Linq.Expressions.Expression.Lambda(notDeleted, parameter);
        }
    }
}

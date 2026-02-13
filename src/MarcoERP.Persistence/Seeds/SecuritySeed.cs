using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MarcoERP.Domain.Entities.Security;

namespace MarcoERP.Persistence.Seeds
{
    /// <summary>
    /// Seeds default roles, permissions, and the admin user.
    /// Idempotent — skips if data already exists.
    /// Per v1.1: 5 roles (Administrator, Accountant, Sales User, Storekeeper, Viewer).
    /// </summary>
    public static class SecuritySeed
    {
        // ── Permission Keys ─────────────────────────────────────
        // Keep in sync with Application/Common/PermissionKeys.cs

        // Accounting
        public const string AccountsView = "accounts.view";
        public const string AccountsCreate = "accounts.create";
        public const string AccountsEdit = "accounts.edit";
        public const string AccountsDelete = "accounts.delete";

        public const string JournalView = "journal.view";
        public const string JournalCreate = "journal.create";
        public const string JournalPost = "journal.post";
        public const string JournalReverse = "journal.reverse";

        public const string FiscalYearManage = "fiscalyear.manage";
        public const string FiscalPeriodManage = "fiscalperiod.manage";

        // Inventory
        public const string InventoryView = "inventory.view";
        public const string InventoryManage = "inventory.manage";

        // Sales
        public const string SalesView = "sales.view";
        public const string SalesCreate = "sales.create";
        public const string SalesPost = "sales.post";

        // Purchases
        public const string PurchasesView = "purchases.view";
        public const string PurchasesCreate = "purchases.create";
        public const string PurchasesPost = "purchases.post";

        // Treasury
        public const string TreasuryView = "treasury.view";
        public const string TreasuryCreate = "treasury.create";
        public const string TreasuryPost = "treasury.post";

        // Reports
        public const string ReportsView = "reports.view";

        // Settings & Admin
        public const string SettingsManage = "settings.manage";
        public const string UsersManage = "users.manage";
        public const string RolesManage = "roles.manage";
        public const string AuditLogView = "auditlog.view";

        // POS
        public const string PosAccess = "pos.access";

        // Governance (Phase 7) — NOT assigned to any default role
        public const string GovernanceAccess = "governance.access";

        /// <summary>All defined permission keys (including governance.access).</summary>
        public static readonly string[] AllPermissions = new[]
        {
            AccountsView, AccountsCreate, AccountsEdit, AccountsDelete,
            JournalView, JournalCreate, JournalPost, JournalReverse,
            FiscalYearManage, FiscalPeriodManage,
            InventoryView, InventoryManage,
            SalesView, SalesCreate, SalesPost,
            PurchasesView, PurchasesCreate, PurchasesPost,
            TreasuryView, TreasuryCreate, TreasuryPost,
            ReportsView,
            SettingsManage, UsersManage, RolesManage, AuditLogView,
            PosAccess,
            GovernanceAccess
        };

        /// <summary>
        /// Seeds roles, permissions, and default admin user.
        /// </summary>
        public static async Task SeedAsync(MarcoDbContext context, string adminPasswordHash)
        {
            // ── Seed Roles ──────────────────────────────────────
            if (!await context.Roles.AnyAsync())
            {
                var administrator = new Role("مدير النظام", "Administrator", "صلاحيات كاملة على جميع الوحدات", isSystem: true);
                var accountant = new Role("محاسب", "Accountant", "إدارة المحاسبة والتقارير المالية", isSystem: true);
                var salesUser = new Role("مسؤول مبيعات", "Sales User", "إدارة عمليات البيع والعملاء", isSystem: true);
                var storekeeper = new Role("أمين مخزن", "Storekeeper", "إدارة المخزون والمنتجات", isSystem: true);
                var viewer = new Role("مشاهد", "Viewer", "عرض فقط بدون صلاحية تعديل", isSystem: true);

                // Administrator: All permissions including governance.access
                foreach (var perm in AllPermissions)
                {
                    administrator.AddPermission(perm);
                }

                // Accountant: Accounting + Fiscal + Treasury + Reports
                foreach (var perm in new[]
                {
                    AccountsView, AccountsCreate, AccountsEdit, AccountsDelete,
                    JournalView, JournalCreate, JournalPost, JournalReverse,
                    FiscalYearManage, FiscalPeriodManage,
                    TreasuryView, TreasuryCreate, TreasuryPost,
                    ReportsView
                })
                    accountant.AddPermission(perm);

                // Sales User: Sales + Reports + POS + Treasury (cash receipts)
                foreach (var perm in new[]
                {
                    SalesView, SalesCreate, SalesPost,
                    ReportsView, PosAccess,
                    TreasuryView, TreasuryCreate, TreasuryPost
                })
                    salesUser.AddPermission(perm);

                // Storekeeper: Inventory + Purchases
                foreach (var perm in new[]
                {
                    InventoryView, InventoryManage,
                    PurchasesView, PurchasesCreate, PurchasesPost
                })
                    storekeeper.AddPermission(perm);

                // Viewer: read-only access
                foreach (var perm in new[]
                {
                    AccountsView, JournalView, InventoryView, SalesView,
                    PurchasesView, TreasuryView, ReportsView
                })
                    viewer.AddPermission(perm);

                context.Roles.AddRange(administrator, accountant, salesUser, storekeeper, viewer);
                await context.SaveChangesAsync();
            }
            else
            {
                // Ensure administrator role has governance.access
                var adminRole = await context.Roles
                    .Include(r => r.Permissions)
                    .FirstOrDefaultAsync(r => r.NameEn == "Administrator");
                if (adminRole != null && !adminRole.HasPermission(GovernanceAccess))
                {
                    adminRole.AddPermission(GovernanceAccess);
                    await context.SaveChangesAsync();
                }
            }

            // ── Seed Default Users ──────────────────────────────
            var administratorRole = await context.Roles.FirstOrDefaultAsync(r => r.NameEn == "Administrator");
            if (administratorRole == null)
                return; // Roles must be seeded first

            if (!await context.Users.AnyAsync())
            {
                // Create default users with BCrypt hashes
                // admin: Admin@123456
                // super: LOLO9090..
                
                var adminUser = new User(
                    username: "admin",
                    passwordHash: adminPasswordHash, // Admin@123456
                    fullNameAr: "مدير النظام",
                    fullNameEn: "System Administrator",
                    email: "admin@marco-erp.local",
                    phone: null,
                    roleId: administratorRole.Id,
                    mustChangePassword: false);

                // Super user with LOLO9090..
                var superPasswordHash = BCrypt.Net.BCrypt.HashPassword("LOLO9090..", workFactor: 12);
                var superUser = new User(
                    username: "super",
                    passwordHash: superPasswordHash,
                    fullNameAr: "المدير الأعلى",
                    fullNameEn: "Super Administrator",
                    email: "super@marco-erp.local",
                    phone: null,
                    roleId: administratorRole.Id,
                    mustChangePassword: false);

                await context.Users.AddRangeAsync(adminUser, superUser);
                await context.SaveChangesAsync();
            }
            else
            {
                // Update/ensure correct passwords for existing users
                var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Username == "admin");
                if (adminUser != null)
                {
                    // Verify password is Admin@123456
                    if (!BCrypt.Net.BCrypt.Verify("Admin@123456", adminUser.PasswordHash))
                    {
                        adminUser.ChangePassword(adminPasswordHash);
                    }
                    // Ensure active and unlocked
                    if (!adminUser.IsActive) adminUser.Activate();
                    if (adminUser.IsLocked) adminUser.Unlock();
                }

                // Ensure super user exists
                var superUser = await context.Users.FirstOrDefaultAsync(u => u.Username == "super");
                if (superUser == null)
                {
                    var superPasswordHash = BCrypt.Net.BCrypt.HashPassword("LOLO9090..", workFactor: 12);
                    superUser = new User(
                        username: "super",
                        passwordHash: superPasswordHash,
                        fullNameAr: "المدير الأعلى",
                        fullNameEn: "Super Administrator",
                        email: "super@marco-erp.local",
                        phone: null,
                        roleId: administratorRole.Id,
                        mustChangePassword: false);
                    await context.Users.AddAsync(superUser);
                }
                else
                {
                    // Update super user password if needed
                    if (!BCrypt.Net.BCrypt.Verify("LOLO9090..", superUser.PasswordHash))
                    {
                        var superPasswordHash = BCrypt.Net.BCrypt.HashPassword("LOLO9090..", workFactor: 12);
                        superUser.ChangePassword(superPasswordHash);
                    }
                    // Ensure active and unlocked
                    if (!superUser.IsActive) superUser.Activate();
                    if (superUser.IsLocked) superUser.Unlock();
                }

                await context.SaveChangesAsync();
            }
        }
    }
}

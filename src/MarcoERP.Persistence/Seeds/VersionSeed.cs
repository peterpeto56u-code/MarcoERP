using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MarcoERP.Domain;
using MarcoERP.Domain.Entities.Settings;

namespace MarcoERP.Persistence.Seeds
{
    /// <summary>
    /// Seeds the initial system version and feature-version mappings.
    /// Idempotent — skips if versions already exist.
    /// Phase 5: Version &amp; Integrity Engine.
    /// </summary>
    public static class VersionSeed
    {
        public static async Task SeedAsync(MarcoDbContext context)
        {
            // ── Seed SystemVersion 1.0.0 ─────────────────────────
            if (!await context.SystemVersions.AnyAsync())
            {
                context.SystemVersions.Add(new SystemVersion(
                    "1.0.0",
                    Domain.DomainConstants.SystemUser,
                    "الإصدار الأول — يشمل: المحاسبة، المخزون، المبيعات، المشتريات، الخزينة، نقاط البيع، التقارير، إدارة المستخدمين",
                    DateTime.UtcNow));
                await context.SaveChangesAsync();
            }

            // ── Seed FeatureVersion mappings ──────────────────────
            if (!await context.FeatureVersions.AnyAsync())
            {
                var mappings = new FeatureVersion[]
                {
                    new("Accounting",     "1.0.0"),
                    new("Inventory",      "1.0.0"),
                    new("Sales",          "1.0.0"),
                    new("Purchases",      "1.0.0"),
                    new("Treasury",       "1.0.0"),
                    new("POS",            "1.0.0"),
                    new("Reporting",      "1.0.0"),
                    new("UserManagement", "1.0.0"),
                };

                await context.FeatureVersions.AddRangeAsync(mappings);
                await context.SaveChangesAsync();
            }
        }
    }
}

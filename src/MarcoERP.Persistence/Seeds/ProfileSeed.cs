using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MarcoERP.Domain.Entities.Settings;

namespace MarcoERP.Persistence.Seeds
{
    /// <summary>
    /// Seeds complexity profiles and their feature mappings.
    /// Idempotent — skips if profiles already exist.
    /// Phase 3: Progressive Complexity Layer.
    /// </summary>
    public static class ProfileSeed
    {
        public static async Task SeedAsync(MarcoDbContext context)
        {
            if (await context.SystemProfiles.AnyAsync())
                return;

            // ── Create Profiles ─────────────────────────────────
            var simple = new SystemProfile(
                "Simple",
                "بروفايل بسيط — المبيعات والمخزون والخزينة فقط",
                false);

            var standard = new SystemProfile(
                "Standard",
                "بروفايل قياسي — يشمل المشتريات والتقارير ونقاط البيع",
                true); // Active by default

            var advanced = new SystemProfile(
                "Advanced",
                "بروفايل متقدم — جميع الميزات مفعّلة بما يشمل المحاسبة المتقدمة",
                false);

            await context.SystemProfiles.AddRangeAsync(simple, standard, advanced);
            await context.SaveChangesAsync();

            // ── Map Features to Profiles ────────────────────────

            // Simple: basic operations
            var simpleFeatures = new[]
            {
                new ProfileFeature(simple.Id, "Accounting"),
                new ProfileFeature(simple.Id, "Inventory"),
                new ProfileFeature(simple.Id, "Sales"),
                new ProfileFeature(simple.Id, "Treasury"),
                new ProfileFeature(simple.Id, "UserManagement"),
            };

            // Standard: Simple + Purchases, POS, Reporting
            var standardFeatures = new[]
            {
                new ProfileFeature(standard.Id, "Accounting"),
                new ProfileFeature(standard.Id, "Inventory"),
                new ProfileFeature(standard.Id, "Sales"),
                new ProfileFeature(standard.Id, "Treasury"),
                new ProfileFeature(standard.Id, "UserManagement"),
                new ProfileFeature(standard.Id, "Purchases"),
                new ProfileFeature(standard.Id, "POS"),
                new ProfileFeature(standard.Id, "Reporting"),
            };

            // Advanced: all features
            var advancedFeatures = new[]
            {
                new ProfileFeature(advanced.Id, "Accounting"),
                new ProfileFeature(advanced.Id, "Inventory"),
                new ProfileFeature(advanced.Id, "Sales"),
                new ProfileFeature(advanced.Id, "Treasury"),
                new ProfileFeature(advanced.Id, "UserManagement"),
                new ProfileFeature(advanced.Id, "Purchases"),
                new ProfileFeature(advanced.Id, "POS"),
                new ProfileFeature(advanced.Id, "Reporting"),
            };

            await context.ProfileFeatures.AddRangeAsync(simpleFeatures);
            await context.ProfileFeatures.AddRangeAsync(standardFeatures);
            await context.ProfileFeatures.AddRangeAsync(advancedFeatures);
            await context.SaveChangesAsync();
        }
    }
}

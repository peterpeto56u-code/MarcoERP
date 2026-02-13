using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MarcoERP.Domain.Entities.Settings;

namespace MarcoERP.Persistence.Seeds
{
    /// <summary>
    /// Seeds default features for the Feature Governance Engine.
    /// Idempotent — skips if features already exist.
    /// Phase 2: Feature Governance Engine.
    /// </summary>
    public static class FeatureSeed
    {
        public static async Task SeedAsync(MarcoDbContext context)
        {
            if (await context.Features.AnyAsync())
                return;

            var features = new Feature[]
            {
                new("Accounting",       "المحاسبة",         "Accounting",         "النظام المحاسبي الأساسي (قيود - حسابات - سنوات مالية)", true,  "High"),
                new("Inventory",        "المخزون",          "Inventory",          "إدارة المخازن والأصناف والوحدات",                       true,  "Medium"),
                new("Sales",            "المبيعات",         "Sales",              "فواتير البيع والمرتجعات وعروض الأسعار",                 true,  "Medium", "Accounting,Inventory"),
                new("Purchases",        "المشتريات",        "Purchases",          "فواتير الشراء والمرتجعات وعروض الأسعار",                true,  "Medium", "Accounting,Inventory"),
                new("Treasury",         "الخزينة",          "Treasury",           "الصناديق والبنوك وسندات القبض والصرف",                  true,  "Medium", "Accounting"),
                new("POS",              "نقاط البيع",       "Point of Sale",      "شاشة نقاط البيع السريعة",                               true,  "Low",    "Sales,Treasury"),
                new("Reporting",        "التقارير",         "Reporting",          "التقارير المالية والإدارية",                             true,  "Low",    "Accounting"),
                new("UserManagement",   "إدارة المستخدمين", "User Management",    "إدارة المستخدمين والأدوار والصلاحيات",                  true,  "High"),
            };

            // Phase 4: Set impact analysis metadata
            features[0].SetImpactMetadata(affectsData: true,  requiresMigration: false, affectsAccounting: true,  affectsInventory: false, affectsReporting: true,  "تعطيل المحاسبة يؤثر على كل القيود والتقارير المالية");
            features[1].SetImpactMetadata(affectsData: true,  requiresMigration: false, affectsAccounting: false, affectsInventory: true,  affectsReporting: true,  "تعطيل المخزون يؤثر على أرصدة الأصناف وحركات المستودعات");
            features[2].SetImpactMetadata(affectsData: true,  requiresMigration: false, affectsAccounting: true,  affectsInventory: true,  affectsReporting: true,  "تعطيل المبيعات يؤثر على القيود المحاسبية وحركات المخزون");
            features[3].SetImpactMetadata(affectsData: true,  requiresMigration: false, affectsAccounting: true,  affectsInventory: true,  affectsReporting: true,  "تعطيل المشتريات يؤثر على القيود المحاسبية وحركات المخزون");
            features[4].SetImpactMetadata(affectsData: true,  requiresMigration: false, affectsAccounting: true,  affectsInventory: false, affectsReporting: true,  "تعطيل الخزينة يؤثر على سندات القبض والصرف والحسابات البنكية");
            features[5].SetImpactMetadata(affectsData: false, requiresMigration: false, affectsAccounting: true,  affectsInventory: true,  affectsReporting: false, "نقاط البيع واجهة فقط — التعطيل آمن");
            features[6].SetImpactMetadata(affectsData: false, requiresMigration: false, affectsAccounting: false, affectsInventory: false, affectsReporting: true,  "تعطيل التقارير يخفي الشاشات فقط — البيانات لا تتأثر");
            features[7].SetImpactMetadata(affectsData: true,  requiresMigration: false, affectsAccounting: false, affectsInventory: false, affectsReporting: false, "تعطيل إدارة المستخدمين يمنع تعديل الصلاحيات والأدوار");

            await context.Features.AddRangeAsync(features);
            await context.SaveChangesAsync();
        }
    }
}

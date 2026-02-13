using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MarcoERP.Domain.Entities.Inventory;

namespace MarcoERP.Persistence.Seeds
{
    /// <summary>
    /// Seeds common units of measure. Idempotent — skips existing.
    /// </summary>
    public static class UnitSeed
    {
        public static async Task SeedAsync(MarcoDbContext context)
        {
            if (await context.Units.AnyAsync())
                return; // Already seeded

            var units = new[]
            {
                CreateUnit("قطعة", "Piece", "قط.", "PC"),
                CreateUnit("كرتونة", "Carton", "كرت.", "CTN"),
                CreateUnit("علبة", "Box", "علب.", "BOX"),
                CreateUnit("باكت", "Pack", "باكت", "PK"),
                CreateUnit("كيلوجرام", "Kilogram", "كجم", "KG"),
                CreateUnit("جرام", "Gram", "جم", "G"),
                CreateUnit("لتر", "Liter", "لتر", "L"),
                CreateUnit("متر", "Meter", "م", "M"),
                CreateUnit("دستة", "Dozen", "دستة", "DZ"),
            };

            await context.Units.AddRangeAsync(units);
            await context.SaveChangesAsync();
        }

        private static Unit CreateUnit(string nameAr, string nameEn, string abbrAr, string abbrEn)
        {
            // Use reflection to set audit fields since constructor doesn't accept them
            var unit = (Unit)Activator.CreateInstance(typeof(Unit), 
                new object[] { nameAr, nameEn, abbrAr, abbrEn });
            
            // Set audit fields via the base class properties
            unit.CreatedAt = DateTime.UtcNow;
            unit.CreatedBy = "System";
            
            return unit;
        }
    }
}

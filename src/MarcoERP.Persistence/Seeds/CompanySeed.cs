using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MarcoERP.Domain.Entities.Common;

namespace MarcoERP.Persistence.Seeds
{
    /// <summary>
    /// Seeds the default company (Id=1) on first run.
    /// Per SYS-03a: The system operates with a single company currently.
    /// </summary>
    public static class CompanySeed
    {
        /// <summary>
        /// Seeds the default company if it does not already exist.
        /// </summary>
        public static async Task SeedAsync(MarcoDbContext context)
        {
            if (await context.Companies.AnyAsync())
                return;

            var defaultCompany = new Company("DEF", "الشركة الافتراضية", "Default Company");

            // Set audit fields for seed
            var now = DateTime.UtcNow;
            var entry = context.Entry(defaultCompany);
            context.Companies.Add(defaultCompany);
            entry.Property(e => e.CreatedAt).CurrentValue = now;
            entry.Property(e => e.CreatedBy).CurrentValue = "SYSTEM";

            await context.SaveChangesAsync();
        }
    }
}

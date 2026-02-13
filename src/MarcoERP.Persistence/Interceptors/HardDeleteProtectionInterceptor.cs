using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MarcoERP.Domain.Entities.Common;

namespace MarcoERP.Persistence.Interceptors
{
    /// <summary>
    /// EF Core interceptor that prevents hard deletion of any entity inheriting SoftDeletableEntity.
    /// Enforces RECORD_PROTECTION_POLICY: financial and business records must be soft-deleted only.
    /// </summary>
    public sealed class HardDeleteProtectionInterceptor : SaveChangesInterceptor
    {
        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result)
        {
            ThrowIfHardDelete(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            ThrowIfHardDelete(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private static void ThrowIfHardDelete(DbContext context)
        {
            if (context == null) return;

            var deletedEntities = context.ChangeTracker
                .Entries<SoftDeletableEntity>()
                .Where(e => e.State == EntityState.Deleted)
                .ToList();

            if (deletedEntities.Count > 0)
            {
                var entityTypes = string.Join(", ",
                    deletedEntities.Select(e => e.Entity.GetType().Name).Distinct());

                throw new InvalidOperationException(
                    $"Hard deletion of soft-deletable entities is prohibited (RECORD_PROTECTION_POLICY). " +
                    $"Use SoftDelete() instead. Affected types: {entityTypes}");
            }
        }
    }
}

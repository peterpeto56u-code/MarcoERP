using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MarcoERP.Domain.Entities.Treasury;

namespace MarcoERP.Domain.Interfaces.Treasury
{
    /// <summary>
    /// Repository contract for Cashbox entity.
    /// </summary>
    public interface ICashboxRepository : IRepository<Cashbox>
    {
        /// <summary>Checks if a cashbox code already exists.</summary>
        Task<bool> CodeExistsAsync(string code, int? excludeId = null, CancellationToken ct = default);

        /// <summary>Gets the default cashbox.</summary>
        Task<Cashbox> GetDefaultAsync(CancellationToken ct = default);

        /// <summary>Gets all active cashboxes.</summary>
        Task<IReadOnlyList<Cashbox>> GetActiveAsync(CancellationToken ct = default);

        /// <summary>Gets the next auto-generated cashbox code (CBX-####).</summary>
        Task<string> GetNextCodeAsync(CancellationToken ct = default);

        /// <summary>
        /// CSH-03: Gets the current posted GL balance (Debit - Credit) for the cashbox's linked account.
        /// Used to prevent negative cashbox balance on payment posting.
        /// </summary>
        Task<decimal> GetGLBalanceAsync(int cashboxId, CancellationToken ct = default);
    }
}

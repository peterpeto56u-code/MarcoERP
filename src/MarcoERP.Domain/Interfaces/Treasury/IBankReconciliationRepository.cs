using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MarcoERP.Domain.Entities.Treasury;

namespace MarcoERP.Domain.Interfaces.Treasury
{
    /// <summary>
    /// Repository contract for BankReconciliation entity.
    /// </summary>
    public interface IBankReconciliationRepository : IRepository<BankReconciliation>
    {
        /// <summary>Gets a reconciliation with its items loaded.</summary>
        Task<BankReconciliation> GetByIdWithItemsAsync(int id, CancellationToken ct = default);

        /// <summary>Gets all reconciliations for a specific bank account.</summary>
        Task<IReadOnlyList<BankReconciliation>> GetByBankAccountAsync(int bankAccountId, CancellationToken ct = default);

        /// <summary>Gets the latest completed reconciliation for a bank account.</summary>
        Task<BankReconciliation> GetLatestCompletedAsync(int bankAccountId, CancellationToken ct = default);
    }
}

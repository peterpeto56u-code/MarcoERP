using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MarcoERP.Domain.Entities.Treasury;

namespace MarcoERP.Domain.Interfaces.Treasury
{
    /// <summary>
    /// Repository contract for BankAccount entity.
    /// </summary>
    public interface IBankAccountRepository : IRepository<BankAccount>
    {
        /// <summary>Checks if a bank account code already exists.</summary>
        Task<bool> CodeExistsAsync(string code, int? excludeId = null, CancellationToken ct = default);

        /// <summary>Gets the default bank account.</summary>
        Task<BankAccount> GetDefaultAsync(CancellationToken ct = default);

        /// <summary>Gets all active bank accounts.</summary>
        Task<IReadOnlyList<BankAccount>> GetActiveAsync(CancellationToken ct = default);

        /// <summary>Gets the next auto-generated bank account code (BNK-####).</summary>
        Task<string> GetNextCodeAsync(CancellationToken ct = default);
    }
}

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MarcoERP.Domain.Entities.Accounting;

namespace MarcoERP.Domain.Interfaces
{
    /// <summary>
    /// Repository interface for Account entity.
    /// </summary>
    public interface IAccountRepository : IRepository<Account>
    {
        /// <summary>Gets an account by its 4-digit code.</summary>
        Task<Account> GetByCodeAsync(string accountCode, CancellationToken cancellationToken = default);

        /// <summary>Gets all child accounts of the given parent.</summary>
        Task<IReadOnlyList<Account>> GetChildrenAsync(int parentAccountId, CancellationToken cancellationToken = default);

        /// <summary>Gets all leaf accounts (AllowPosting = true, IsActive = true).</summary>
        Task<IReadOnlyList<Account>> GetPostableAccountsAsync(CancellationToken cancellationToken = default);

        /// <summary>Gets all accounts by type.</summary>
        Task<IReadOnlyList<Account>> GetByTypeAsync(Enums.AccountType accountType, CancellationToken cancellationToken = default);

        /// <summary>Checks if an account code already exists.</summary>
        Task<bool> CodeExistsAsync(string accountCode, CancellationToken cancellationToken = default);

        /// <summary>Checks if the account has any children.</summary>
        Task<bool> HasChildrenAsync(int accountId, CancellationToken cancellationToken = default);
    }
}

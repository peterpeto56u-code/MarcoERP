using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace MarcoERP.Domain.Interfaces
{
    /// <summary>
    /// Unit of Work pattern — ensures atomicity of multi-repository operations.
    /// TRX-INT-01: One transaction per use case; multiple SaveChanges are allowed within it.
    /// TRX-INT-02: Application layer initiates, Persistence layer executes.
    /// </summary>
    public interface IUnitOfWork
    {
        /// <summary>Commits all pending changes in a single transaction.</summary>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes the provided operation within a database transaction.
        /// </summary>
        Task ExecuteInTransactionAsync(
            Func<Task> operation,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
            CancellationToken cancellationToken = default);
    }
}

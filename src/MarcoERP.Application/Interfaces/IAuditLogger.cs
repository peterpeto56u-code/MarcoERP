using System.Threading;
using System.Threading.Tasks;

namespace MarcoERP.Application.Interfaces
{
    /// <summary>
    /// Logs user actions to the audit trail.
    /// Implementation: Infrastructure/Persistence layer.
    /// </summary>
    public interface IAuditLogger
    {
        /// <summary>
        /// Logs an auditable action.
        /// </summary>
        /// <param name="entityType">Name of the entity (e.g., "JournalEntry").</param>
        /// <param name="entityId">Primary key of the affected entity.</param>
        /// <param name="action">Action performed (e.g., "Posted", "Created", "Reversed").</param>
        /// <param name="performedBy">Username of the actor.</param>
        /// <param name="details">Optional description or serialized change details.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task LogAsync(
            string entityType,
            int entityId,
            string action,
            string performedBy,
            string details = null,
            CancellationToken cancellationToken = default);
    }
}

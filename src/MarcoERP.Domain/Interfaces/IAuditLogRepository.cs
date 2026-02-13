using System.Threading;
using System.Threading.Tasks;
using MarcoERP.Domain.Entities.Accounting;

namespace MarcoERP.Domain.Interfaces
{
    /// <summary>
    /// Repository interface for AuditLog entity.
    /// Supports append-only operations — no Update or Remove.
    /// </summary>
    public interface IAuditLogRepository
    {
        /// <summary>Adds a new audit log record.</summary>
        Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default);
    }
}

using System.Threading;
using System.Threading.Tasks;
using MarcoERP.Application.Common;

namespace MarcoERP.Application.Interfaces.Settings
{
    /// <summary>
    /// Phase 6: Thin wrapper around IBackupService for migration-specific backups.
    /// Provides a simplified API: CreatePreMigrationBackupAsync(reason).
    /// </summary>
    public interface IDatabaseBackupService
    {
        /// <summary>
        /// Creates a full database backup before migration execution.
        /// Returns the absolute path to the backup file on success.
        /// </summary>
        /// <param name="reason">Reason for backup (e.g. migration name).</param>
        /// <param name="ct">Cancellation token.</param>
        Task<ServiceResult<string>> CreatePreMigrationBackupAsync(string reason, CancellationToken ct = default);
    }
}

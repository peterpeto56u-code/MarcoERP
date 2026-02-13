using System;
using MarcoERP.Domain.Entities.Common;

namespace MarcoERP.Domain.Entities.Settings
{
    /// <summary>
    /// Records each database migration execution attempt.
    /// Phase 6: Controlled Migration Engine — tracking only.
    /// </summary>
    public sealed class MigrationExecution : BaseEntity
    {
        /// <summary>EF Core only.</summary>
        private MigrationExecution() { }

        /// <summary>
        /// Creates a new migration execution record (started state).
        /// </summary>
        public MigrationExecution(string migrationName, string executedBy, DateTime utcNow)
        {
            if (string.IsNullOrWhiteSpace(migrationName))
                throw new ArgumentException("اسم الـ Migration مطلوب.", nameof(migrationName));

            MigrationName = migrationName.Trim();
            StartedAt = utcNow;
            ExecutedBy = executedBy?.Trim() ?? DomainConstants.SystemUser;
            IsSuccessful = false;
        }

        /// <summary>EF Core migration name.</summary>
        public string MigrationName { get; private set; }

        /// <summary>UTC timestamp when execution started.</summary>
        public DateTime StartedAt { get; private set; }

        /// <summary>UTC timestamp when execution completed (null if still running or failed).</summary>
        public DateTime? CompletedAt { get; private set; }

        /// <summary>Whether the migration completed successfully.</summary>
        public bool IsSuccessful { get; private set; }

        /// <summary>Username who triggered the migration.</summary>
        public string ExecutedBy { get; private set; }

        /// <summary>Error message if the migration failed (null if successful).</summary>
        public string ErrorMessage { get; private set; }

        /// <summary>Backup file path created before this migration (null if no backup).</summary>
        public string BackupPath { get; private set; }

        // ── Domain Methods ──────────────────────────────────────

        /// <summary>Marks the migration as successfully completed.</summary>
        public void MarkSuccess(DateTime utcNow)
        {
            IsSuccessful = true;
            CompletedAt = utcNow;
            ErrorMessage = null;
        }

        /// <summary>Marks the migration as failed with an error.</summary>
        public void MarkFailed(string errorMessage, DateTime utcNow)
        {
            IsSuccessful = false;
            CompletedAt = utcNow;
            ErrorMessage = errorMessage?.Trim();
        }

        /// <summary>Records the backup path created before this migration.</summary>
        public void SetBackupPath(string path)
        {
            BackupPath = path?.Trim();
        }
    }
}

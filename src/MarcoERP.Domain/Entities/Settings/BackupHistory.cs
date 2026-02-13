using System;
using MarcoERP.Domain.Entities.Common;

namespace MarcoERP.Domain.Entities.Settings
{
    /// <summary>
    /// Records each backup/restore operation for audit and history.
    /// </summary>
    public sealed class BackupHistory : BaseEntity
    {
        // ── Constructors ────────────────────────────────────────

        /// <summary>EF Core only.</summary>
        private BackupHistory() { }

        /// <summary>
        /// Creates a new backup history record.
        /// </summary>
        public BackupHistory(
            string filePath,
            long fileSizeBytes,
            DateTime backupDate,
            string performedBy,
            string backupType,
            bool isSuccessful,
            string errorMessage = null)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("مسار النسخة الاحتياطية مطلوب.", nameof(filePath));
            if (string.IsNullOrWhiteSpace(performedBy))
                throw new ArgumentException("اسم المستخدم مطلوب.", nameof(performedBy));
            if (string.IsNullOrWhiteSpace(backupType))
                throw new ArgumentException("نوع النسخة الاحتياطية مطلوب.", nameof(backupType));

            FilePath = filePath.Trim();
            FileSizeBytes = fileSizeBytes;
            BackupDate = backupDate;
            PerformedBy = performedBy.Trim();
            BackupType = backupType.Trim();
            IsSuccessful = isSuccessful;
            ErrorMessage = errorMessage?.Trim();
            CreatedAt = backupDate;
        }

        // ── Properties ──────────────────────────────────────────

        /// <summary>Full path to the backup file.</summary>
        public string FilePath { get; private set; }

        /// <summary>Size of the backup file in bytes.</summary>
        public long FileSizeBytes { get; private set; }

        /// <summary>Date and time the backup was performed.</summary>
        public DateTime BackupDate { get; private set; }

        /// <summary>Username of the user who performed the backup.</summary>
        public string PerformedBy { get; private set; }

        /// <summary>Backup type: Full or Differential.</summary>
        public string BackupType { get; private set; }

        /// <summary>Whether the backup completed successfully.</summary>
        public bool IsSuccessful { get; private set; }

        /// <summary>Error message if the backup failed.</summary>
        public string ErrorMessage { get; private set; }

        /// <summary>Record creation timestamp.</summary>
        public DateTime CreatedAt { get; private set; }
    }
}

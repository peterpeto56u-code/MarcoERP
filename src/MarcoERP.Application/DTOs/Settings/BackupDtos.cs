using System;

namespace MarcoERP.Application.DTOs.Settings
{
    /// <summary>
    /// Result returned after a successful backup operation.
    /// </summary>
    public sealed class BackupResultDto
    {
        public int Id { get; set; }
        public string FilePath { get; set; }
        public long FileSizeBytes { get; set; }
        public DateTime BackupDate { get; set; }
        public string PerformedBy { get; set; }
    }

    /// <summary>
    /// Represents a single backup history record.
    /// </summary>
    public sealed class BackupHistoryDto
    {
        public int Id { get; set; }
        public string FilePath { get; set; }
        public long FileSizeBytes { get; set; }
        public DateTime BackupDate { get; set; }
        public string PerformedBy { get; set; }
        /// <summary>Full or Differential.</summary>
        public string BackupType { get; set; }
        public bool IsSuccessful { get; set; }
        public string ErrorMessage { get; set; }
    }
}

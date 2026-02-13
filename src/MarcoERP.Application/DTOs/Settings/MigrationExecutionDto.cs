using System;

namespace MarcoERP.Application.DTOs.Settings
{
    /// <summary>
    /// Phase 6: DTO for migration execution history display.
    /// </summary>
    public sealed class MigrationExecutionDto
    {
        public int Id { get; set; }
        public string MigrationName { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public bool IsSuccessful { get; set; }
        public string ExecutedBy { get; set; }
        public string ErrorMessage { get; set; }
        public string BackupPath { get; set; }

        /// <summary>Duration in seconds (null if not completed).</summary>
        public double? DurationSeconds =>
            CompletedAt.HasValue ? (CompletedAt.Value - StartedAt).TotalSeconds : (double?)null;
    }
}

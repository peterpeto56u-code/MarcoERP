using System;

namespace MarcoERP.Application.DTOs.Settings
{
    /// <summary>
    /// DTO for audit log records displayed in the Audit Log Viewer.
    /// AUD-02: Includes OldValues/NewValues/ChangedColumns per DATABASE_POLICY.
    /// </summary>
    public sealed class AuditLogDto
    {
        public long Id { get; set; }
        public string EntityType { get; set; }
        public int EntityId { get; set; }
        public string Action { get; set; }
        public string PerformedBy { get; set; }
        public string Details { get; set; }
        public DateTime Timestamp { get; set; }
        public string OldValues { get; set; }
        public string NewValues { get; set; }
        public string ChangedColumns { get; set; }
    }
}

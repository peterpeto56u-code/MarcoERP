using System;

namespace MarcoERP.Domain.Entities.Accounting
{
    /// <summary>
    /// Immutable audit log record. One row per auditable action.
    /// Inserted within the same transaction as the business operation (TRX-INT-05).
    /// AUD-02: Captures OldValues, NewValues, ChangedColumns per DATABASE_POLICY.
    /// </summary>
    public sealed class AuditLog
    {
        /// <summary>EF Core only.</summary>
        private AuditLog() { }

        public AuditLog(
            string entityType,
            int entityId,
            string action,
            string performedBy,
            DateTime timestamp,
            string details = null,
            string oldValues = null,
            string newValues = null,
            string changedColumns = null)
        {
            EntityType = entityType;
            EntityId = entityId;
            Action = action;
            PerformedBy = performedBy;
            Details = details;
            Timestamp = timestamp;
            OldValues = oldValues;
            NewValues = newValues;
            ChangedColumns = changedColumns;
        }

        /// <summary>Primary key.</summary>
        public long Id { get; private set; }

        /// <summary>Name of the entity affected (e.g., "JournalEntry").</summary>
        public string EntityType { get; private set; }

        /// <summary>Primary key of the affected entity.</summary>
        public int EntityId { get; private set; }

        /// <summary>Action performed (e.g., "Posted", "Created").</summary>
        public string Action { get; private set; }

        /// <summary>Username of the actor.</summary>
        public string PerformedBy { get; private set; }

        /// <summary>Additional details or serialized change info.</summary>
        public string Details { get; private set; }

        /// <summary>UTC timestamp of the action.</summary>
        public DateTime Timestamp { get; private set; }

        /// <summary>JSON of previous values (null on Create). AUD-02 compliance.</summary>
        public string OldValues { get; private set; }

        /// <summary>JSON of new values. AUD-02 compliance.</summary>
        public string NewValues { get; private set; }

        /// <summary>JSON array of changed column names. AUD-02 compliance.</summary>
        public string ChangedColumns { get; private set; }
    }
}

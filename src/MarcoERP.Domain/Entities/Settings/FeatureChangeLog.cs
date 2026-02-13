using System;
using MarcoERP.Domain.Entities.Common;

namespace MarcoERP.Domain.Entities.Settings
{
    /// <summary>
    /// Audit log for feature enable/disable changes.
    /// Phase 2: Feature Governance Engine.
    /// </summary>
    public sealed class FeatureChangeLog : BaseEntity
    {
        // ── Constructors ────────────────────────────────────────

        /// <summary>EF Core only.</summary>
        private FeatureChangeLog() { }

        /// <summary>
        /// Creates a new feature change log entry.
        /// </summary>
        public FeatureChangeLog(int featureId, string featureKey, bool oldValue, bool newValue, string changedBy, DateTime changedAt)
        {
            FeatureId = featureId;
            FeatureKey = featureKey?.Trim();
            OldValue = oldValue;
            NewValue = newValue;
            ChangedBy = changedBy?.Trim();
            ChangedAt = changedAt;
        }

        // ── Properties ──────────────────────────────────────────

        /// <summary>FK to the Feature that was changed.</summary>
        public int FeatureId { get; private set; }

        /// <summary>Feature key at the time of change (denormalized for audit trail).</summary>
        public string FeatureKey { get; private set; }

        /// <summary>Previous enabled state.</summary>
        public bool OldValue { get; private set; }

        /// <summary>New enabled state.</summary>
        public bool NewValue { get; private set; }

        /// <summary>Username who made the change.</summary>
        public string ChangedBy { get; private set; }

        /// <summary>UTC timestamp of the change.</summary>
        public System.DateTime ChangedAt { get; private set; }

        // ── Navigation ──────────────────────────────────────────

        /// <summary>Navigation property to the Feature.</summary>
        public Feature Feature { get; private set; }
    }
}

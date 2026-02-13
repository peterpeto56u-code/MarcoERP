using System;
using MarcoERP.Domain.Enums;

namespace MarcoERP.Application.Reporting.Models
{
    /// <summary>
    /// Base class for all interactive report rows.
    /// Carries drill-down metadata so the UI can navigate to source documents.
    /// </summary>
    public abstract class ReportRowBase
    {
        /// <summary>Unique row identifier (entity PK or composite key hash).</summary>
        public int RowId { get; set; }

        /// <summary>The source document type this row originated from.</summary>
        public SourceType? SourceType { get; set; }

        /// <summary>The primary-key of the source entity (e.g. InvoiceId, JournalEntryId).</summary>
        public int? SourceId { get; set; }

        /// <summary>The ERP module this row belongs to (Accounting, Sales, Purchases, etc.).</summary>
        public string Module { get; set; }

        /// <summary>
        /// Pre-resolved navigation key for the drill-down engine.
        /// If null, the DrillDownResolver will resolve from <see cref="SourceType"/>.
        /// </summary>
        public string NavigationTarget { get; set; }

        /// <summary>Whether this row supports expanding into child rows.</summary>
        public virtual bool IsExpandable => false;

        /// <summary>Display-level indentation for hierarchical rows (0 = root).</summary>
        public int Level { get; set; }
    }
}

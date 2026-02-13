using System.Collections.Generic;

namespace MarcoERP.Application.Reporting.Models
{
    /// <summary>
    /// A report row that can expand to reveal child detail rows.
    /// Used for hierarchical reports (e.g. Account → JournalEntries, Invoice → Lines).
    /// </summary>
    public abstract class ExpandableReportRow<TDetail> : ReportRowBase
        where TDetail : ReportRowBase
    {
        /// <summary>Whether this row is currently expanded in the UI.</summary>
        public bool IsExpanded { get; set; }

        /// <summary>Whether child rows have been loaded from the server.</summary>
        public bool IsChildrenLoaded { get; set; }

        /// <summary>Whether children are currently loading.</summary>
        public bool IsLoadingChildren { get; set; }

        /// <summary>Lazily-loaded child rows.</summary>
        public List<TDetail> Children { get; set; } = new();

        /// <inheritdoc />
        public override bool IsExpandable => true;
    }
}

using System.Collections.Generic;

namespace MarcoERP.Application.Reporting.Models
{
    /// <summary>
    /// Complete metadata for a report: title, columns, available filters, default sort.
    /// Returned by each report ViewModel to configure the ReportViewBase control.
    /// </summary>
    public sealed class ReportDefinition
    {
        /// <summary>Arabic report title (e.g. "ميزان المراجعة").</summary>
        public string Title { get; set; }

        /// <summary>Optional Arabic subtitle.</summary>
        public string Subtitle { get; set; }

        /// <summary>Navigation key for this report (e.g. "TrialBalance").</summary>
        public string ReportKey { get; set; }

        /// <summary>Column definitions in display order.</summary>
        public List<ReportColumnDefinition> Columns { get; set; } = new();

        /// <summary>Available filters for this report.</summary>
        public List<FilterDefinition> Filters { get; set; } = new();

        /// <summary>Default sort column and direction.</summary>
        public SortDefinition DefaultSort { get; set; }

        /// <summary>Default page size.</summary>
        public int DefaultPageSize { get; set; } = 50;

        /// <summary>Available page size options.</summary>
        public List<int> PageSizeOptions { get; set; } = new() { 25, 50, 100, 200 };

        /// <summary>Whether rows support drill-down navigation.</summary>
        public bool SupportsDrillDown { get; set; }

        /// <summary>Whether rows support expansion into child rows.</summary>
        public bool SupportsExpandableRows { get; set; }

        /// <summary>Whether the report supports PDF export.</summary>
        public bool SupportsPdfExport { get; set; } = true;

        /// <summary>Whether the report supports Excel export.</summary>
        public bool SupportsExcelExport { get; set; } = true;

        /// <summary>Saved filter presets for this report.</summary>
        public List<FilterPreset> Presets { get; set; } = new();
    }
}

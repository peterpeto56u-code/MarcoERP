using System.Collections.Generic;

namespace MarcoERP.Application.DTOs.Reports
{
    /// <summary>
    /// Generic DTO for exporting tabular report data to PDF/Excel.
    /// </summary>
    public sealed class ReportExportRequest
    {
        /// <summary>Report title (e.g. "ميزان المراجعة").</summary>
        public string Title { get; set; }

        /// <summary>Optional subtitle (e.g. date range).</summary>
        public string Subtitle { get; set; }

        /// <summary>Column headers.</summary>
        public List<ReportColumn> Columns { get; set; } = new();

        /// <summary>Data rows — each row is a list of cell values.</summary>
        public List<List<string>> Rows { get; set; } = new();

        /// <summary>Optional footer summary (e.g. "إجمالي المدين: 1,000").</summary>
        public string FooterSummary { get; set; }
    }

    /// <summary>Column definition for report export.</summary>
    public sealed class ReportColumn
    {
        public string Header { get; set; }
        public float WidthRatio { get; set; } = 1f;
        public bool IsNumeric { get; set; }

        public ReportColumn() { }
        public ReportColumn(string header, float widthRatio = 1f, bool isNumeric = false)
        {
            Header = header;
            WidthRatio = widthRatio;
            IsNumeric = isNumeric;
        }
    }
}

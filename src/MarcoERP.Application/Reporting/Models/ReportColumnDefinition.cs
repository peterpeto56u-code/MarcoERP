namespace MarcoERP.Application.Reporting.Models
{
    /// <summary>
    /// Defines metadata for a single report column: header, binding, type, visibility rules.
    /// </summary>
    public sealed class ReportColumnDefinition
    {
        /// <summary>Arabic display header.</summary>
        public string Header { get; set; }

        /// <summary>Property name on the row DTO for data binding.</summary>
        public string BindingPath { get; set; }

        /// <summary>Column data type for formatting and sorting.</summary>
        public ColumnDataType DataType { get; set; } = ColumnDataType.Text;

        /// <summary>Relative width ratio (1.0 = standard, 2.0 = double, 0.5 = half).</summary>
        public double WidthRatio { get; set; } = 1.0;

        /// <summary>Whether this column is sortable by the user.</summary>
        public bool IsSortable { get; set; } = true;

        /// <summary>Whether this column is filterable.</summary>
        public bool IsFilterable { get; set; }

        /// <summary>Minimum complexity mode required to show this column.</summary>
        public ReportComplexityMode MinComplexity { get; set; } = ReportComplexityMode.Simple;

        /// <summary>String format for numeric/date display (e.g. "N2", "yyyy/MM/dd").</summary>
        public string StringFormat { get; set; }

        /// <summary>Whether to right-align (for numeric columns in RTL layout).</summary>
        public bool IsNumeric { get; set; }

        /// <summary>Fixed pixel width. If null, uses WidthRatio.</summary>
        public double? FixedWidth { get; set; }
    }

    public enum ColumnDataType
    {
        Text,
        Integer,
        Decimal,
        Currency,
        Date,
        DateTime,
        Boolean,
        Percentage
    }

    /// <summary>
    /// Controls column/filter visibility based on user sophistication level.
    /// </summary>
    public enum ReportComplexityMode
    {
        /// <summary>Essential columns only — for cashiers and basic users.</summary>
        Simple = 0,

        /// <summary>Standard columns — for accountants and supervisors.</summary>
        Moderate = 1,

        /// <summary>All columns including audit trails — for auditors and admins.</summary>
        Advanced = 2
    }
}

namespace MarcoERP.Application.Reporting.Models
{
    /// <summary>
    /// A summary KPI card displayed at the top of a report.
    /// </summary>
    public sealed class KpiCard
    {
        /// <summary>Arabic label (e.g. "إجمالي المبيعات").</summary>
        public string Title { get; set; }

        /// <summary>Formatted display value (e.g. "125,000.00").</summary>
        public string Value { get; set; }

        /// <summary>Optional subtitle or unit (e.g. "ر.س" or "فاتورة").</summary>
        public string Subtitle { get; set; }

        /// <summary>Material Design icon kind name.</summary>
        public string IconKind { get; set; }

        /// <summary>Card accent color key (maps to theme brush).</summary>
        public KpiColorTheme ColorTheme { get; set; } = KpiColorTheme.Primary;

        /// <summary>Optional trend indicator: positive = up, negative = down, zero = neutral.</summary>
        public decimal? TrendPercent { get; set; }

        /// <summary>Minimum complexity mode to show this KPI.</summary>
        public ReportComplexityMode MinComplexity { get; set; } = ReportComplexityMode.Simple;
    }

    public enum KpiColorTheme
    {
        Primary,
        Success,
        Warning,
        Danger,
        Info,
        Neutral
    }
}

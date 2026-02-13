namespace MarcoERP.Application.DTOs.Reports
{
    /// <summary>
    /// DTO for a single row in the Profit Report (per product).
    /// </summary>
    public sealed class ProfitReportRowDto
    {
        public int ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public decimal TotalSalesQuantity { get; set; }
        public decimal TotalSalesAmount { get; set; }
        public decimal TotalCostAmount { get; set; }
        public decimal GrossProfit { get; set; }
        public decimal ProfitMarginPercent { get; set; }
    }

    /// <summary>
    /// Wrapper for the full Profit Report with totals.
    /// </summary>
    public sealed class ProfitReportDto
    {
        public decimal TotalSales { get; set; }
        public decimal TotalCost { get; set; }
        public decimal TotalProfit { get; set; }
        public decimal OverallMarginPercent { get; set; }
        public System.Collections.Generic.List<ProfitReportRowDto> Rows { get; set; } = new();
    }
}

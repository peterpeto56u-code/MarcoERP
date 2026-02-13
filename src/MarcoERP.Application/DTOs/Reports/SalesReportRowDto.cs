using System;

namespace MarcoERP.Application.DTOs.Reports
{
    /// <summary>
    /// DTO for a single row in the Sales Report.
    /// </summary>
    public sealed class SalesReportRowDto
    {
        public int InvoiceId { get; set; }
        public string InvoiceNumber { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string CustomerName { get; set; }
        public string Status { get; set; }
        public decimal Subtotal { get; set; }
        public decimal DiscountTotal { get; set; }
        public decimal VatTotal { get; set; }
        public decimal NetTotal { get; set; }
    }

    /// <summary>
    /// Wrapper for the full Sales Report with summary totals.
    /// </summary>
    public sealed class SalesReportDto
    {
        public decimal TotalSubtotal { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal TotalVat { get; set; }
        public decimal TotalNet { get; set; }
        public int InvoiceCount { get; set; }
        public System.Collections.Generic.List<SalesReportRowDto> Rows { get; set; } = new();
    }
}

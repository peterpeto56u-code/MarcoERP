using System;

namespace MarcoERP.Application.DTOs.Reports
{
    /// <summary>
    /// DTO for a single row in the Purchase Report.
    /// </summary>
    public sealed class PurchaseReportRowDto
    {
        public int InvoiceId { get; set; }
        public string InvoiceNumber { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string SupplierName { get; set; }
        public string Status { get; set; }
        public decimal Subtotal { get; set; }
        public decimal DiscountTotal { get; set; }
        public decimal VatTotal { get; set; }
        public decimal NetTotal { get; set; }
    }

    /// <summary>
    /// Wrapper for the full Purchase Report with summary totals.
    /// </summary>
    public sealed class PurchaseReportDto
    {
        public decimal TotalSubtotal { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal TotalVat { get; set; }
        public decimal TotalNet { get; set; }
        public int InvoiceCount { get; set; }
        public System.Collections.Generic.List<PurchaseReportRowDto> Rows { get; set; } = new();
    }
}

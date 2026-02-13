namespace MarcoERP.Application.DTOs.Reports
{
    /// <summary>
    /// DTO for a single row in the VAT Report (per VAT rate).
    /// </summary>
    public sealed class VatReportRowDto
    {
        public decimal VatRate { get; set; }
        public decimal SalesBase { get; set; }
        public decimal SalesVat { get; set; }
        public decimal PurchaseBase { get; set; }
        public decimal PurchaseVat { get; set; }
        public decimal NetVat { get; set; }
    }

    /// <summary>
    /// Full VAT Report wrapper with totals.
    /// </summary>
    public sealed class VatReportDto
    {
        public decimal TotalSalesBase { get; set; }
        public decimal TotalSalesVat { get; set; }
        public decimal TotalPurchaseBase { get; set; }
        public decimal TotalPurchaseVat { get; set; }
        public decimal NetVatPayable { get; set; }
        public System.Collections.Generic.List<VatReportRowDto> Rows { get; set; } = new();
    }
}

namespace MarcoERP.Application.DTOs.Reports
{
    /// <summary>
    /// DTO for a single row in the Aging Report (customers or suppliers).
    /// </summary>
    public sealed class AgingRowDto
    {
        public int EntityId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public decimal Current { get; set; }       // 0-30 days
        public decimal Days30 { get; set; }         // 31-60 days
        public decimal Days60 { get; set; }         // 61-90 days
        public decimal Days90 { get; set; }         // 91-120 days
        public decimal Days120Plus { get; set; }    // 120+ days
        public decimal Total { get; set; }
    }

    /// <summary>
    /// Full Aging Report with separate customer and supplier sections.
    /// </summary>
    public sealed class AgingReportDto
    {
        public System.Collections.Generic.List<AgingRowDto> CustomerAging { get; set; } = new();
        public System.Collections.Generic.List<AgingRowDto> SupplierAging { get; set; } = new();
        public decimal TotalCustomerBalance { get; set; }
        public decimal TotalSupplierBalance { get; set; }
    }
}

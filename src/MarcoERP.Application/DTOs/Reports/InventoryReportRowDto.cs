namespace MarcoERP.Application.DTOs.Reports
{
    /// <summary>
    /// DTO for a single row in the Inventory Report (stock per warehouse per product).
    /// </summary>
    public sealed class InventoryReportRowDto
    {
        public int ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string CategoryName { get; set; }
        public string WarehouseName { get; set; }
        public string UnitName { get; set; }
        public decimal Quantity { get; set; }
        public decimal CostPrice { get; set; }
        public decimal TotalValue { get; set; }
        public decimal MinimumStock { get; set; }
        public bool IsBelowMinimum { get; set; }
    }
}

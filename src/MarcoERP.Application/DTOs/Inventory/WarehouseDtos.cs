namespace MarcoERP.Application.DTOs.Inventory
{
    // ════════════════════════════════════════════════════════════
    //  Warehouse DTOs
    // ════════════════════════════════════════════════════════════

    public sealed class WarehouseDto
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public int? AccountId { get; set; }
        public string AccountName { get; set; }
        public bool IsActive { get; set; }
        public bool IsDefault { get; set; }
    }

    public sealed class CreateWarehouseDto
    {
        public string Code { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public int? AccountId { get; set; }
    }

    public sealed class UpdateWarehouseDto
    {
        public int Id { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public int? AccountId { get; set; }
    }

    /// <summary>Stock balance for a product in a warehouse.</summary>
    public sealed class StockBalanceDto
    {
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public decimal Quantity { get; set; }
        public string BaseUnitName { get; set; }
        public decimal MinimumStock { get; set; }
        public bool IsBelowMinimum => Quantity < MinimumStock;
    }
}

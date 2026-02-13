using System;

namespace MarcoERP.Application.DTOs.Reports
{
    /// <summary>
    /// DTO for a single row in the Stock Card report (item movement history).
    /// </summary>
    public sealed class StockCardRowDto
    {
        public DateTime MovementDate { get; set; }
        public string MovementTypeName { get; set; }
        public string ReferenceNumber { get; set; }
        public string SourceTypeName { get; set; }
        public string WarehouseName { get; set; }
        public decimal QuantityIn { get; set; }
        public decimal QuantityOut { get; set; }
        public decimal UnitCost { get; set; }
        public decimal BalanceAfter { get; set; }
    }

    /// <summary>
    /// Wrapper for the Stock Card report with product info.
    /// </summary>
    public sealed class StockCardReportDto
    {
        public int ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string UnitName { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal TotalIn { get; set; }
        public decimal TotalOut { get; set; }
        public decimal ClosingBalance { get; set; }
        public System.Collections.Generic.List<StockCardRowDto> Rows { get; set; } = new();
    }
}

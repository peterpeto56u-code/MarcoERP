using System;
using System.Collections.Generic;

namespace MarcoERP.Application.DTOs.Sales
{
    // ═══════════════════════════════════════════════════════════
    //  POS DTOs
    // ═══════════════════════════════════════════════════════════

    // ── Session DTOs ────────────────────────────────────────────

    /// <summary>DTO for opening a POS session.</summary>
    public sealed class OpenPosSessionDto
    {
        public int CashboxId { get; set; }
        public int WarehouseId { get; set; }
        public decimal OpeningBalance { get; set; }
    }

    /// <summary>DTO for closing a POS session.</summary>
    public sealed class ClosePosSessionDto
    {
        public int SessionId { get; set; }
        public decimal ActualClosingBalance { get; set; }
        public string Notes { get; set; }
    }

    /// <summary>Read-only DTO for POS session display.</summary>
    public sealed class PosSessionDto
    {
        public int Id { get; set; }
        public string SessionNumber { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public int CashboxId { get; set; }
        public string CashboxNameAr { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseNameAr { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal TotalSales { get; set; }
        public decimal TotalCashReceived { get; set; }
        public decimal TotalCardReceived { get; set; }
        public decimal TotalOnAccount { get; set; }
        public int TransactionCount { get; set; }
        public decimal ClosingBalance { get; set; }
        public decimal Variance { get; set; }
        public string Status { get; set; }
        public DateTime OpenedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public string ClosingNotes { get; set; }
    }

    /// <summary>Lightweight session list item.</summary>
    public sealed class PosSessionListDto
    {
        public int Id { get; set; }
        public string SessionNumber { get; set; }
        public string UserName { get; set; }
        public string Status { get; set; }
        public decimal TotalSales { get; set; }
        public int TransactionCount { get; set; }
        public DateTime OpenedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
    }

    // ── POS Sale DTOs ───────────────────────────────────────────

    /// <summary>DTO for completing a POS sale.</summary>
    public sealed class CompletePoseSaleDto
    {
        public int SessionId { get; set; }
        public int? CustomerId { get; set; }
        public string Notes { get; set; }
        public List<PosSaleLineDto> Lines { get; set; } = new();
        public List<PosPaymentDto> Payments { get; set; } = new();
    }

    /// <summary>DTO for a POS sale line item.</summary>
    public sealed class PosSaleLineDto
    {
        public int ProductId { get; set; }
        public int UnitId { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountPercent { get; set; }
    }

    /// <summary>DTO for a POS payment entry.</summary>
    public sealed class PosPaymentDto
    {
        public string PaymentMethod { get; set; }
        public decimal Amount { get; set; }
        public string ReferenceNumber { get; set; }
    }

    // ── Product Lookup DTOs (lightweight for POS cache) ─────────

    /// <summary>Lightweight product DTO for POS product cache.</summary>
    public sealed class PosProductLookupDto
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string NameAr { get; set; }
        public string Barcode { get; set; }
        public decimal DefaultSalePrice { get; set; }
        public decimal WeightedAverageCost { get; set; }
        public decimal VatRate { get; set; }
        public List<PosProductUnitDto> Units { get; set; } = new();
    }

    /// <summary>Lightweight product unit DTO for POS.</summary>
    public sealed class PosProductUnitDto
    {
        public int UnitId { get; set; }
        public string UnitNameAr { get; set; }
        public decimal ConversionFactor { get; set; }
        public decimal SalePrice { get; set; }
        public string Barcode { get; set; }
        public bool IsDefault { get; set; }
    }

    // ── POS Cart Item (client-side ViewModel DTO) ───────────────

    /// <summary>Represents a line in the POS cart for UI display.</summary>
    public sealed class PosCartItemDto
    {
        public int ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductNameAr { get; set; }
        public int UnitId { get; set; }
        public string UnitNameAr { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal ConversionFactor { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal VatRate { get; set; }
        public decimal AvailableStock { get; set; }
        public decimal WacPerBaseUnit { get; set; }

        // ── Calculated Properties (populated via ILineCalculationService) ──
        public decimal BaseQuantity { get; set; }
        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal NetTotal { get; set; }
        public decimal VatAmount { get; set; }
        public decimal TotalWithVat { get; set; }
        public decimal CostTotal { get; set; }
        public decimal ProfitAmount { get; set; }
        public decimal ProfitMarginPercent { get; set; }
    }

    // ── POS Report DTOs ─────────────────────────────────────────

    /// <summary>POS daily report DTO.</summary>
    public sealed class PosDailyReportDto
    {
        public DateTime Date { get; set; }
        public int TotalTransactions { get; set; }
        public decimal TotalSales { get; set; }
        public decimal TotalCash { get; set; }
        public decimal TotalCard { get; set; }
        public decimal TotalOnAccount { get; set; }
        public decimal TotalCogs { get; set; }
        public decimal GrossProfit { get; set; }
    }

    /// <summary>POS session report DTO.</summary>
    public sealed class PosSessionReportDto
    {
        public PosSessionDto Session { get; set; }
        public List<PosSessionSaleDto> Sales { get; set; } = new();
    }

    /// <summary>A sale within a session report.</summary>
    public sealed class PosSessionSaleDto
    {
        public string InvoiceNumber { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string CustomerNameAr { get; set; }
        public decimal NetTotal { get; set; }
        public string PaymentMethods { get; set; }
    }

    /// <summary>POS profit report DTO.</summary>
    public sealed class PosProfitReportDto
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalCogs { get; set; }
        public decimal GrossProfit { get; set; }
        public decimal GrossProfitMarginPercent { get; set; }
        public List<PosProfitLineDto> Lines { get; set; } = new();
    }

    /// <summary>Per-product profit line in the profit report.</summary>
    public sealed class PosProfitLineDto
    {
        public string ProductCode { get; set; }
        public string ProductNameAr { get; set; }
        public decimal QuantitySold { get; set; }
        public decimal Revenue { get; set; }
        public decimal Cost { get; set; }
        public decimal Profit { get; set; }
        public decimal ProfitMarginPercent { get; set; }
    }

    /// <summary>Cash variance report DTO.</summary>
    public sealed class CashVarianceReportDto
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public List<CashVarianceLineDto> Lines { get; set; } = new();
        public decimal TotalVariance { get; set; }
    }

    /// <summary>Per-session cash variance line.</summary>
    public sealed class CashVarianceLineDto
    {
        public string SessionNumber { get; set; }
        public string UserName { get; set; }
        public DateTime OpenedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal TotalCashReceived { get; set; }
        public decimal ExpectedBalance { get; set; }
        public decimal ClosingBalance { get; set; }
        public decimal Variance { get; set; }
    }
}

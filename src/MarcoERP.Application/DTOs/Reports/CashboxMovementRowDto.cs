using System;

namespace MarcoERP.Application.DTOs.Reports
{
    /// <summary>
    /// DTO for a single row in the Cashbox Movement report.
    /// </summary>
    public sealed class CashboxMovementRowDto
    {
        public DateTime Date { get; set; }
        public string DocumentType { get; set; }  // سند قبض / سند صرف / تحويل
        public string DocumentNumber { get; set; }
        public string Description { get; set; }
        public string CounterpartyName { get; set; }
        public decimal AmountIn { get; set; }
        public decimal AmountOut { get; set; }
        public decimal RunningBalance { get; set; }
    }

    /// <summary>
    /// Wrapper for the Cashbox Movement report.
    /// </summary>
    public sealed class CashboxMovementReportDto
    {
        public int? CashboxId { get; set; }
        public string CashboxName { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal TotalIn { get; set; }
        public decimal TotalOut { get; set; }
        public decimal ClosingBalance { get; set; }
        public System.Collections.Generic.List<CashboxMovementRowDto> Rows { get; set; } = new();
    }
}

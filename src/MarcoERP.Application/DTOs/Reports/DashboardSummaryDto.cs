namespace MarcoERP.Application.DTOs.Reports
{
    /// <summary>
    /// DTO for the main Dashboard summary data.
    /// Provides today's snapshot + period totals.
    /// </summary>
    public sealed class DashboardSummaryDto
    {
        // ── Today's Figures ──
        public decimal TodaySales { get; set; }
        public decimal TodayPurchases { get; set; }
        public decimal TodayReceipts { get; set; }
        public decimal TodayPayments { get; set; }
        public int TodaySalesCount { get; set; }
        public int TodayPurchasesCount { get; set; }

        // ── Period Totals (current month) ──
        public decimal MonthSales { get; set; }
        public decimal MonthPurchases { get; set; }
        public decimal MonthReceipts { get; set; }
        public decimal MonthPayments { get; set; }

        // ── Overall Balances ──
        public decimal TotalCustomerBalance { get; set; }
        public decimal TotalSupplierBalance { get; set; }
        public decimal CashBalance { get; set; }
        public decimal MonthGrossProfit { get; set; }

        // ── Inventory Alerts ──
        public int LowStockCount { get; set; }
        public int TotalProducts { get; set; }

        // ── Drafts Pending ──
        public int PendingSalesInvoices { get; set; }
        public int PendingPurchaseInvoices { get; set; }
        public int PendingJournalEntries { get; set; }
    }
}

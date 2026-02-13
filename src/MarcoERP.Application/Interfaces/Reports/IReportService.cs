using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MarcoERP.Application.Common;
using MarcoERP.Application.DTOs.Reports;

namespace MarcoERP.Application.Interfaces.Reports
{
    /// <summary>
    /// Application service for all reporting queries.
    /// Read-only — no data modifications.
    /// All date-range parameters are inclusive.
    /// </summary>
    public interface IReportService
    {
        // ── Accounting Reports ──────────────────────────────────

        /// <summary>
        /// Gets the Trial Balance for a date range.
        /// Aggregates posted journal entry lines by account.
        /// </summary>
        Task<ServiceResult<IReadOnlyList<TrialBalanceRowDto>>> GetTrialBalanceAsync(
            DateTime fromDate, DateTime toDate, CancellationToken ct = default);

        /// <summary>
        /// Gets the Account Statement showing all movements for a specific account.
        /// Includes running balance.
        /// </summary>
        Task<ServiceResult<AccountStatementReportDto>> GetAccountStatementAsync(
            int accountId, DateTime fromDate, DateTime toDate, CancellationToken ct = default);

        /// <summary>
        /// Gets the Income Statement (Profit &amp; Loss) for a date range.
        /// Revenue - COGS = Gross Profit - Expenses + Other Income - Other Expenses = Net Profit.
        /// </summary>
        Task<ServiceResult<IncomeStatementDto>> GetIncomeStatementAsync(
            DateTime fromDate, DateTime toDate, CancellationToken ct = default);

        /// <summary>
        /// Gets the Balance Sheet as of a specific date.
        /// Assets = Liabilities + Equity.
        /// </summary>
        Task<ServiceResult<BalanceSheetDto>> GetBalanceSheetAsync(
            DateTime asOfDate, CancellationToken ct = default);

        // ── Sales & Purchase Reports ────────────────────────────

        /// <summary>
        /// Gets a Sales Report for a date range, optionally filtered by customer.
        /// Includes posted invoices only.
        /// </summary>
        Task<ServiceResult<SalesReportDto>> GetSalesReportAsync(
            DateTime fromDate, DateTime toDate, int? customerId = null, CancellationToken ct = default);

        /// <summary>
        /// Gets a Purchase Report for a date range, optionally filtered by supplier.
        /// Includes posted invoices only.
        /// </summary>
        Task<ServiceResult<PurchaseReportDto>> GetPurchaseReportAsync(
            DateTime fromDate, DateTime toDate, int? supplierId = null, CancellationToken ct = default);

        /// <summary>
        /// Gets the Profit Report per product for a date range.
        /// Compares sales revenue vs cost of goods sold.
        /// </summary>
        Task<ServiceResult<ProfitReportDto>> GetProfitReportAsync(
            DateTime fromDate, DateTime toDate, CancellationToken ct = default);

        // ── Inventory Reports ───────────────────────────────────

        /// <summary>
        /// Gets the Inventory Report showing current stock levels.
        /// Optionally filtered by warehouse.
        /// </summary>
        Task<ServiceResult<IReadOnlyList<InventoryReportRowDto>>> GetInventoryReportAsync(
            int? warehouseId = null, CancellationToken ct = default);

        /// <summary>
        /// Gets the Stock Card for a specific product showing all movements.
        /// Optionally filtered by warehouse.
        /// </summary>
        Task<ServiceResult<StockCardReportDto>> GetStockCardAsync(
            int productId, int? warehouseId, DateTime fromDate, DateTime toDate,
            CancellationToken ct = default);

        // ── Treasury Reports ────────────────────────────────────

        /// <summary>
        /// Gets the Cashbox Movement report.
        /// Shows all receipts, payments, and transfers for a cashbox.
        /// </summary>
        Task<ServiceResult<CashboxMovementReportDto>> GetCashboxMovementAsync(
            int? cashboxId, DateTime fromDate, DateTime toDate, CancellationToken ct = default);

        // ── Aging & VAT ─────────────────────────────────────────

        /// <summary>
        /// Gets the Aging Report for customer and supplier receivables/payables.
        /// Grouped by 30-day buckets.
        /// </summary>
        Task<ServiceResult<AgingReportDto>> GetAgingReportAsync(CancellationToken ct = default);

        /// <summary>
        /// Gets the VAT Report for a date range.
        /// Shows sales VAT collected vs purchase VAT paid.
        /// </summary>
        Task<ServiceResult<VatReportDto>> GetVatReportAsync(
            DateTime fromDate, DateTime toDate, CancellationToken ct = default);

        // ── Dashboard ───────────────────────────────────────────

        /// <summary>
        /// Gets the Dashboard summary with key business metrics.
        /// </summary>
        Task<ServiceResult<DashboardSummaryDto>> GetDashboardSummaryAsync(CancellationToken ct = default);
    }
}

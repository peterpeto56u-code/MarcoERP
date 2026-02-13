using System;
using System.Threading;
using System.Threading.Tasks;

namespace MarcoERP.Application.Interfaces.SmartEntry
{
    /// <summary>
    /// Read-only, performance-oriented queries used by invoice Smart Entry UX.
    /// Kept separate from transactional services to avoid loading full aggregates.
    /// </summary>
    public interface ISmartEntryQueryService
    {
        Task<decimal> GetStockBaseQtyAsync(int warehouseId, int productId, CancellationToken ct = default);

        Task<decimal?> GetLastSalesUnitPriceAsync(int customerId, int productId, int unitId, CancellationToken ct = default);

        Task<decimal?> GetLastPurchaseUnitPriceAsync(int productId, int unitId, CancellationToken ct = default);

        Task<decimal?> GetLastPurchaseUnitPriceForSupplierAsync(int supplierId, int productId, int unitId, CancellationToken ct = default);

        Task<decimal> GetPostedAccountBalanceAsync(int accountId, CancellationToken ct = default);

        /// <summary>
        /// Calculates the customer's outstanding sales balance as:
        /// SUM(Posted SalesInvoices.NetTotal) - SUM(Posted CashReceipts.Amount linked to those invoices).
        /// </summary>
        Task<decimal> GetCustomerOutstandingSalesBalanceAsync(int customerId, CancellationToken ct = default);

        /// <summary>
        /// Resolves the best tier price (base-unit) for a customer/product at the given base-unit quantity.
        /// Priority:
        /// 1) Customer-specific price list (if assigned and valid)
        /// 2) Lowest price across all active/valid lists
        /// Returns null when no applicable tier exists.
        /// </summary>
        Task<decimal?> GetBestTierSaleBaseUnitPriceForCustomerAsync(
            int customerId,
            int productId,
            decimal baseUnitQuantity,
            CancellationToken ct = default);

        Task<bool> HasOverduePostedSalesInvoicesAsync(int customerId, DateTime cutoffDate, CancellationToken ct = default);
    }
}

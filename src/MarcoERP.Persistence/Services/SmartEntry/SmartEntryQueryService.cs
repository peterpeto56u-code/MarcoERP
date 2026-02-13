using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MarcoERP.Application.Interfaces;
using MarcoERP.Application.Interfaces.SmartEntry;
using MarcoERP.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MarcoERP.Persistence.Services.SmartEntry
{
    public sealed class SmartEntryQueryService : ISmartEntryQueryService
    {
        private readonly MarcoDbContext _db;
        private readonly IDateTimeProvider _dateTime;

        public SmartEntryQueryService(MarcoDbContext db, IDateTimeProvider dateTime)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _dateTime = dateTime ?? throw new ArgumentNullException(nameof(dateTime));
        }

        private static async Task<(bool HasValue, T Value)> TryFirstAsync<T>(IAsyncEnumerable<T> source, CancellationToken ct)
        {
            if (source == null)
                return (false, default(T));

            await using var enumerator = source.GetAsyncEnumerator(ct);
            return await enumerator.MoveNextAsync()
                ? (true, enumerator.Current)
                : (false, default(T));
        }

        public async Task<decimal> GetStockBaseQtyAsync(int warehouseId, int productId, CancellationToken ct = default)
        {
            var result = await TryFirstAsync(CompiledQueries.GetWarehouseProductQuantityBase(_db, warehouseId, productId), ct);
            return result.HasValue ? result.Value : 0m;
        }

        public async Task<decimal?> GetLastSalesUnitPriceAsync(int customerId, int productId, int unitId, CancellationToken ct = default)
        {
            var result = await TryFirstAsync(CompiledQueries.GetLastSalesUnitPriceForCustomer(_db, customerId, productId, unitId), ct);
            return result.HasValue ? result.Value : (decimal?)null;
        }

        public async Task<decimal?> GetLastPurchaseUnitPriceAsync(int productId, int unitId, CancellationToken ct = default)
        {
            var result = await TryFirstAsync(CompiledQueries.GetLastPurchaseUnitPrice(_db, productId, unitId), ct);
            return result.HasValue ? result.Value : (decimal?)null;
        }

        public async Task<decimal?> GetLastPurchaseUnitPriceForSupplierAsync(int supplierId, int productId, int unitId, CancellationToken ct = default)
        {
            var result = await TryFirstAsync(CompiledQueries.GetLastPurchaseUnitPriceForSupplier(_db, supplierId, productId, unitId), ct);
            return result.HasValue ? result.Value : (decimal?)null;
        }

        public async Task<decimal> GetPostedAccountBalanceAsync(int accountId, CancellationToken ct = default)
        {
            var result = await TryFirstAsync(CompiledQueries.GetPostedAccountBalance(_db, accountId), ct);
            return result.HasValue ? result.Value : 0m;
        }

        public async Task<decimal> GetCustomerOutstandingSalesBalanceAsync(int customerId, CancellationToken ct = default)
        {
            if (customerId <= 0) return 0m;

            var invoicesTotal = await _db.SalesInvoices
                .AsNoTracking()
                .Where(i => i.Status == InvoiceStatus.Posted && i.CustomerId == customerId)
                .Select(i => (decimal?)i.NetTotal)
                .SumAsync(ct) ?? 0m;

            // Receipts are linked by SalesInvoiceId; join to ensure we only count receipts for this customer's invoices.
            var receiptsTotal = await (
                    from r in _db.CashReceipts.AsNoTracking()
                    join i in _db.SalesInvoices.AsNoTracking() on r.SalesInvoiceId equals i.Id
                    where r.Status == InvoiceStatus.Posted
                          && i.Status == InvoiceStatus.Posted
                          && i.CustomerId == customerId
                    select (decimal?)r.Amount)
                .SumAsync(ct) ?? 0m;

            return invoicesTotal - receiptsTotal;
        }

        public async Task<decimal?> GetBestTierSaleBaseUnitPriceForCustomerAsync(
            int customerId,
            int productId,
            decimal baseUnitQuantity,
            CancellationToken ct = default)
        {
            if (customerId <= 0 || productId <= 0)
                return null;

            if (baseUnitQuantity < 0)
                baseUnitQuantity = 0;

            var today = _dateTime.UtcNow.Date;

            var customerPriceListId = await _db.Customers
                .AsNoTracking()
                .Where(c => c.Id == customerId)
                .Select(c => c.PriceListId)
                .FirstOrDefaultAsync(ct);

            if (customerPriceListId.HasValue)
            {
                var customerTierPrice = await (
                        from p in _db.PriceLists.AsNoTracking()
                        join t in _db.PriceTiers.AsNoTracking() on p.Id equals t.PriceListId
                        where p.Id == customerPriceListId.Value
                              && p.IsActive
                              && (!p.ValidFrom.HasValue || p.ValidFrom.Value <= today)
                              && (!p.ValidTo.HasValue || p.ValidTo.Value >= today)
                              && t.ProductId == productId
                              && t.MinimumQuantity <= baseUnitQuantity
                        orderby t.MinimumQuantity descending
                        select (decimal?)t.Price)
                    .FirstOrDefaultAsync(ct);

                if (customerTierPrice.HasValue)
                    return customerTierPrice.Value;
            }

            // Across all active lists: pick the applied tier per list (max min-qty <= requested),
            // then take the lowest price among lists.
            var bestMinQtyPerList =
                from p in _db.PriceLists.AsNoTracking()
                join t in _db.PriceTiers.AsNoTracking() on p.Id equals t.PriceListId
                where p.IsActive
                      && (!p.ValidFrom.HasValue || p.ValidFrom.Value <= today)
                      && (!p.ValidTo.HasValue || p.ValidTo.Value >= today)
                      && t.ProductId == productId
                      && t.MinimumQuantity <= baseUnitQuantity
                group t by t.PriceListId
                into g
                select new
                {
                    PriceListId = g.Key,
                    BestMinimumQuantity = g.Max(x => x.MinimumQuantity)
                };

            var bestTierPrices =
                from best in bestMinQtyPerList
                join t in _db.PriceTiers.AsNoTracking()
                    on new { best.PriceListId, best.BestMinimumQuantity, ProductId = productId }
                    equals new { t.PriceListId, BestMinimumQuantity = t.MinimumQuantity, t.ProductId }
                select (decimal?)t.Price;

            return await bestTierPrices.MinAsync(ct);
        }

        public async Task<bool> HasOverduePostedSalesInvoicesAsync(int customerId, DateTime cutoffDate, CancellationToken ct = default)
        {
            var result = await TryFirstAsync(CompiledQueries.GetAnyOverduePostedSalesInvoiceId(_db, customerId, cutoffDate), ct);
            return result.HasValue;
        }
    }
}

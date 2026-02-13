using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using MarcoERP.Application.DTOs.Search;
using MarcoERP.Domain.Entities.Accounting;
using MarcoERP.Domain.Entities.Inventory;
using MarcoERP.Domain.Entities.Purchases;
using MarcoERP.Domain.Entities.Sales;
using MarcoERP.Domain.Entities.Treasury;
using MarcoERP.Domain.Enums;

namespace MarcoERP.Persistence
{
    /// <summary>
    /// Pre-compiled EF Core queries for frequently-executed operations.
    /// Eliminates expression-tree compilation overhead on every call.
    /// Phase C.3 — Performance Hardening.
    /// </summary>
    internal static class CompiledQueries
    {
        // ── Accounting ──────────────────────────────────────────

        /// <summary>
        /// Retrieves a single Account by its unique AccountCode.
        /// Used by chart-of-accounts lookups and journal posting validation.
        /// </summary>
        public static readonly Func<MarcoDbContext, string, IAsyncEnumerable<Account>> GetAccountByCode =
            EF.CompileAsyncQuery(
                (MarcoDbContext db, string code) =>
                    db.Accounts.Where(a => a.AccountCode == code).Take(1));

        /// <summary>
        /// Retrieves the currently-active FiscalYear (Status == Active).
        /// At most one fiscal year is active at any time (FY-INV-03).
        /// </summary>
        public static readonly Func<MarcoDbContext, IAsyncEnumerable<FiscalYear>> GetActiveFiscalYear =
            EF.CompileAsyncQuery(
                (MarcoDbContext db) =>
                    db.FiscalYears
                      .Include(fy => fy.Periods)
                      .Where(fy => fy.Status == FiscalYearStatus.Active)
                      .Take(1));

        // ── Inventory ───────────────────────────────────────────

        /// <summary>
        /// Retrieves a Product by its unique Code (with base unit eagerly loaded).
        /// Used by invoice line entry and product search.
        /// </summary>
        public static readonly Func<MarcoDbContext, string, IAsyncEnumerable<Product>> GetProductByCode =
            EF.CompileAsyncQuery(
                (MarcoDbContext db, string code) =>
                    db.Products
                      .Include(p => p.BaseUnit)
                      .Where(p => p.Code == code)
                      .Take(1));

        /// <summary>
        /// Retrieves a Product by Barcode (with base unit eagerly loaded).
        /// Used by POS barcode scanning and quick search.
        /// </summary>
        public static readonly Func<MarcoDbContext, string, IAsyncEnumerable<Product>> GetProductByBarcode =
            EF.CompileAsyncQuery(
                (MarcoDbContext db, string barcode) =>
                    db.Products
                      .Include(p => p.BaseUnit)
                      .Where(p => p.Barcode == barcode)
                      .Take(1));

        /// <summary>
        /// Retrieves a WarehouseProduct stock record by warehouse + product composite key.
        /// Used by every inventory movement to read / update stock balance.
        /// </summary>
        public static readonly Func<MarcoDbContext, int, int, IAsyncEnumerable<WarehouseProduct>> GetWarehouseProduct =
            EF.CompileAsyncQuery(
                (MarcoDbContext db, int warehouseId, int productId) =>
                    db.WarehouseProducts
                      .Where(wp => wp.WarehouseId == warehouseId && wp.ProductId == productId)
                      .Take(1));

        /// <summary>
        /// Retrieves WarehouseProduct.Quantity (base unit) for warehouse + product.
        /// Used by Smart Entry (stock display).
        /// </summary>
        public static readonly Func<MarcoDbContext, int, int, IAsyncEnumerable<decimal>> GetWarehouseProductQuantityBase =
            EF.CompileAsyncQuery(
                (MarcoDbContext db, int warehouseId, int productId) =>
                    db.WarehouseProducts
                      .AsNoTracking()
                      .Where(wp => wp.WarehouseId == warehouseId && wp.ProductId == productId)
                      .Select(wp => wp.Quantity)
                      .Take(1));

        // ── Smart Entry: Last Prices ──────────────────────────

        public static readonly Func<MarcoDbContext, int, int, int, IAsyncEnumerable<decimal>> GetLastSalesUnitPriceForCustomer =
            EF.CompileAsyncQuery(
                (MarcoDbContext db, int customerId, int productId, int unitId) =>
                    (from l in db.Set<SalesInvoiceLine>().AsNoTracking()
                     join h in db.Set<SalesInvoice>().AsNoTracking() on l.SalesInvoiceId equals h.Id
                     where h.Status == InvoiceStatus.Posted
                           && h.CustomerId == customerId
                           && l.ProductId == productId
                           && l.UnitId == unitId
                     orderby h.InvoiceDate descending, h.Id descending
                     select l.UnitPrice)
                    .Take(1));

        public static readonly Func<MarcoDbContext, int, int, IAsyncEnumerable<decimal>> GetLastPurchaseUnitPrice =
            EF.CompileAsyncQuery(
                (MarcoDbContext db, int productId, int unitId) =>
                    (from l in db.Set<PurchaseInvoiceLine>().AsNoTracking()
                     join h in db.Set<PurchaseInvoice>().AsNoTracking() on l.PurchaseInvoiceId equals h.Id
                     where h.Status == InvoiceStatus.Posted
                           && l.ProductId == productId
                           && l.UnitId == unitId
                     orderby h.InvoiceDate descending, h.Id descending
                     select l.UnitPrice)
                    .Take(1));

        public static readonly Func<MarcoDbContext, int, int, int, IAsyncEnumerable<decimal>> GetLastPurchaseUnitPriceForSupplier =
            EF.CompileAsyncQuery(
                (MarcoDbContext db, int supplierId, int productId, int unitId) =>
                    (from l in db.Set<PurchaseInvoiceLine>().AsNoTracking()
                     join h in db.Set<PurchaseInvoice>().AsNoTracking() on l.PurchaseInvoiceId equals h.Id
                     where h.Status == InvoiceStatus.Posted
                           && h.SupplierId == supplierId
                           && l.ProductId == productId
                           && l.UnitId == unitId
                     orderby h.InvoiceDate descending, h.Id descending
                     select l.UnitPrice)
                    .Take(1));

        // ── Smart Entry: Customer Financial Status ────────────

        public static readonly Func<MarcoDbContext, int, IAsyncEnumerable<decimal>> GetPostedAccountBalance =
            EF.CompileAsyncQuery(
                (MarcoDbContext db, int accountId) =>
                    (from l in db.Set<JournalEntryLine>().AsNoTracking()
                     join h in db.Set<JournalEntry>().AsNoTracking() on l.JournalEntryId equals h.Id
                     where h.Status == JournalEntryStatus.Posted
                           && l.AccountId == accountId
                     select (l.DebitAmount - l.CreditAmount))
                    .GroupBy(_ => 1)
                    .Select(g => g.Sum())
                    .Take(1));

        public static readonly Func<MarcoDbContext, int, DateTime, IAsyncEnumerable<int>> GetAnyOverduePostedSalesInvoiceId =
            EF.CompileAsyncQuery(
                (MarcoDbContext db, int customerId, DateTime cutoffDate) =>
                    db.Set<SalesInvoice>()
                      .AsNoTracking()
                      .Where(i => i.Status == InvoiceStatus.Posted
                                  && i.CustomerId == customerId
                                  && i.InvoiceDate < cutoffDate)
                      .Select(i => i.Id)
                      .Take(1));

        // ── Global Search (Ctrl+K) ─────────────────────────────

        public static readonly Func<MarcoDbContext, string, int, IAsyncEnumerable<GlobalSearchHitDto>> SearchCustomers =
            EF.CompileAsyncQuery(
                (MarcoDbContext db, string like, int take) =>
                    db.Set<Customer>()
                      .AsNoTracking()
                      .Where(c => EF.Functions.Like(c.Code, like)
                                  || EF.Functions.Like(c.NameAr, like)
                                  || (c.Phone != null && EF.Functions.Like(c.Phone, like))
                                  || (c.Mobile != null && EF.Functions.Like(c.Mobile, like)))
                      .OrderBy(c => c.NameAr)
                      .Select(c => new GlobalSearchHitDto
                      {
                          EntityType = GlobalSearchEntityType.Customer,
                          Id = c.Id,
                          Title = c.Code + " - " + c.NameAr,
                          Subtitle = c.Phone ?? c.Mobile,
                      })
                      .Take(take));

        public static readonly Func<MarcoDbContext, string, int, IAsyncEnumerable<GlobalSearchHitDto>> SearchProducts =
            EF.CompileAsyncQuery(
                (MarcoDbContext db, string like, int take) =>
                    db.Set<Product>()
                      .AsNoTracking()
                      .Where(p => EF.Functions.Like(p.Code, like)
                                  || EF.Functions.Like(p.NameAr, like)
                                  || (p.Barcode != null && EF.Functions.Like(p.Barcode, like)))
                      .OrderBy(p => p.NameAr)
                      .Select(p => new GlobalSearchHitDto
                      {
                          EntityType = GlobalSearchEntityType.Product,
                          Id = p.Id,
                          Title = p.Code + " - " + p.NameAr,
                          Subtitle = p.Barcode,
                      })
                      .Take(take));

        public static readonly Func<MarcoDbContext, string, int, IAsyncEnumerable<GlobalSearchHitDto>> SearchSalesInvoices =
            EF.CompileAsyncQuery(
                (MarcoDbContext db, string like, int take) =>
                    db.Set<SalesInvoice>()
                      .AsNoTracking()
                      .Where(i => EF.Functions.Like(i.InvoiceNumber, like))
                      .OrderByDescending(i => i.InvoiceDate)
                      .Select(i => new GlobalSearchHitDto
                      {
                          EntityType = GlobalSearchEntityType.SalesInvoice,
                          Id = i.Id,
                          Title = i.InvoiceNumber,
                          Subtitle = i.Status.ToString(),
                      })
                      .Take(take));

        public static readonly Func<MarcoDbContext, string, int, IAsyncEnumerable<GlobalSearchHitDto>> SearchPurchaseInvoices =
            EF.CompileAsyncQuery(
                (MarcoDbContext db, string like, int take) =>
                    db.Set<PurchaseInvoice>()
                      .AsNoTracking()
                      .Where(i => EF.Functions.Like(i.InvoiceNumber, like))
                      .OrderByDescending(i => i.InvoiceDate)
                      .Select(i => new GlobalSearchHitDto
                      {
                          EntityType = GlobalSearchEntityType.PurchaseInvoice,
                          Id = i.Id,
                          Title = i.InvoiceNumber,
                          Subtitle = i.Status.ToString(),
                      })
                      .Take(take));

        public static readonly Func<MarcoDbContext, string, int, IAsyncEnumerable<GlobalSearchHitDto>> SearchJournalEntries =
            EF.CompileAsyncQuery(
                (MarcoDbContext db, string like, int take) =>
                    db.Set<JournalEntry>()
                      .AsNoTracking()
                      .Where(j => (j.JournalNumber != null && EF.Functions.Like(j.JournalNumber, like))
                                  || (j.Description != null && EF.Functions.Like(j.Description, like)))
                      .OrderByDescending(j => j.JournalDate)
                      .Select(j => new GlobalSearchHitDto
                      {
                          EntityType = GlobalSearchEntityType.JournalEntry,
                          Id = j.Id,
                          Title = j.JournalNumber ?? ("JE-" + j.Id),
                          Subtitle = j.Description,
                      })
                      .Take(take));

        public static readonly Func<MarcoDbContext, string, int, IAsyncEnumerable<GlobalSearchHitDto>> SearchCashReceipts =
            EF.CompileAsyncQuery(
                (MarcoDbContext db, string like, int take) =>
                    db.Set<CashReceipt>()
                      .AsNoTracking()
                      .Where(r => EF.Functions.Like(r.ReceiptNumber, like)
                                  || (r.Description != null && EF.Functions.Like(r.Description, like)))
                      .OrderByDescending(r => r.ReceiptDate)
                      .Select(r => new GlobalSearchHitDto
                      {
                          EntityType = GlobalSearchEntityType.CashReceipt,
                          Id = r.Id,
                          Title = r.ReceiptNumber,
                          Subtitle = r.Description,
                      })
                      .Take(take));

        public static readonly Func<MarcoDbContext, string, int, IAsyncEnumerable<GlobalSearchHitDto>> SearchCashPayments =
            EF.CompileAsyncQuery(
                (MarcoDbContext db, string like, int take) =>
                    db.Set<CashPayment>()
                      .AsNoTracking()
                      .Where(p => EF.Functions.Like(p.PaymentNumber, like)
                                  || (p.Description != null && EF.Functions.Like(p.Description, like)))
                      .OrderByDescending(p => p.PaymentDate)
                      .Select(p => new GlobalSearchHitDto
                      {
                          EntityType = GlobalSearchEntityType.CashPayment,
                          Id = p.Id,
                          Title = p.PaymentNumber,
                          Subtitle = p.Description,
                      })
                      .Take(take));

        public static readonly Func<MarcoDbContext, string, int, IAsyncEnumerable<GlobalSearchHitDto>> SearchSuppliers =
            EF.CompileAsyncQuery(
                (MarcoDbContext db, string like, int take) =>
                    db.Set<Supplier>()
                      .AsNoTracking()
                      .Where(s => EF.Functions.Like(s.Code, like)
                                  || EF.Functions.Like(s.NameAr, like)
                                  || (s.Phone != null && EF.Functions.Like(s.Phone, like))
                                  || (s.Mobile != null && EF.Functions.Like(s.Mobile, like)))
                      .OrderBy(s => s.NameAr)
                      .Select(s => new GlobalSearchHitDto
                      {
                          EntityType = GlobalSearchEntityType.Supplier,
                          Id = s.Id,
                          Title = s.Code + " - " + s.NameAr,
                          Subtitle = s.Phone ?? s.Mobile,
                      })
                      .Take(take));
    }
}

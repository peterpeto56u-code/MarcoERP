using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MarcoERP.Application.Common;
using MarcoERP.Application.DTOs.Settings;
using MarcoERP.Application.Interfaces;
using MarcoERP.Application.Interfaces.Settings;
using MarcoERP.Domain.Enums;

namespace MarcoERP.Persistence.Services
{
    /// <summary>
    /// Persistence-layer integrity service.
    /// Runs data-quality checks using EF Core LINQ queries against SQL Server.
    /// </summary>
    public sealed class IntegrityService : IIntegrityService
    {
        private readonly MarcoDbContext _context;
        private readonly IDateTimeProvider _dateTime;

        public IntegrityService(MarcoDbContext context, IDateTimeProvider dateTime)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dateTime = dateTime ?? throw new ArgumentNullException(nameof(dateTime));
        }

        /// <inheritdoc />
        public async Task<ServiceResult<TrialBalanceCheckResult>> CheckTrialBalanceAsync(CancellationToken ct = default)
        {
            try
            {
                // Get IDs of all posted journal entries
                var postedEntryIds = _context.JournalEntries
                    .Where(je => je.Status == JournalEntryStatus.Posted)
                    .Select(je => je.Id);

                // Sum debits & credits per account across all posted journal entries
                var accountTotals = await _context.JournalEntryLines
                    .Where(l => postedEntryIds.Contains(l.JournalEntryId))
                    .GroupBy(l => l.AccountId)
                    .Select(g => new
                    {
                        AccountId = g.Key,
                        Debit = g.Sum(l => l.DebitAmount),
                        Credit = g.Sum(l => l.CreditAmount)
                    })
                    .ToListAsync(ct);

                var totalDebits = accountTotals.Sum(a => a.Debit);
                var totalCredits = accountTotals.Sum(a => a.Credit);
                var difference = totalDebits - totalCredits;
                var isBalanced = difference == 0m;

                var unbalancedAccounts = new List<UnbalancedAccountDto>();

                // If the trial balance is not balanced, load account details for reporting
                if (!isBalanced)
                {
                    var accountIds = accountTotals.Select(a => a.AccountId).ToList();

                    var accounts = await _context.Accounts
                        .Where(a => accountIds.Contains(a.Id))
                        .Select(a => new { a.Id, a.AccountCode, a.AccountNameAr })
                        .ToDictionaryAsync(a => a.Id, ct);

                    unbalancedAccounts = accountTotals
                        .Where(a => a.Debit != a.Credit)
                        .Select(a =>
                        {
                            accounts.TryGetValue(a.AccountId, out var acct);
                            return new UnbalancedAccountDto
                            {
                                AccountId = a.AccountId,
                                AccountCode = acct?.AccountCode ?? "—",
                                AccountName = acct?.AccountNameAr ?? "—",
                                Debit = a.Debit,
                                Credit = a.Credit
                            };
                        })
                        .ToList();
                }

                return ServiceResult<TrialBalanceCheckResult>.Success(new TrialBalanceCheckResult
                {
                    IsBalanced = isBalanced,
                    TotalDebits = totalDebits,
                    TotalCredits = totalCredits,
                    Difference = Math.Abs(difference),
                    UnbalancedAccounts = unbalancedAccounts
                });
            }
            catch (Exception ex)
            {
                return ServiceResult<TrialBalanceCheckResult>.Failure($"خطأ في فحص ميزان المراجعة: {ex.Message}");
            }
        }

        /// <inheritdoc />
        public async Task<ServiceResult<JournalBalanceCheckResult>> CheckJournalBalancesAsync(CancellationToken ct = default)
        {
            try
            {
                // For each posted journal entry, compare sum of DR vs CR from lines
                var entryTotals = await _context.JournalEntries
                    .Where(je => je.Status == JournalEntryStatus.Posted)
                    .Select(je => new
                    {
                        je.Id,
                        je.JournalNumber,
                        Debit = je.Lines.Sum(l => l.DebitAmount),
                        Credit = je.Lines.Sum(l => l.CreditAmount)
                    })
                    .ToListAsync(ct);

                var totalChecked = entryTotals.Count;

                var unbalancedEntries = entryTotals
                    .Where(e => e.Debit != e.Credit)
                    .Select(e => new UnbalancedJournalEntryDto
                    {
                        JournalEntryId = e.Id,
                        JournalNumber = e.JournalNumber ?? "—",
                        Debit = e.Debit,
                        Credit = e.Credit,
                        Difference = Math.Abs(e.Debit - e.Credit)
                    })
                    .ToList();

                return ServiceResult<JournalBalanceCheckResult>.Success(new JournalBalanceCheckResult
                {
                    AllBalanced = unbalancedEntries.Count == 0,
                    TotalChecked = totalChecked,
                    UnbalancedCount = unbalancedEntries.Count,
                    UnbalancedEntries = unbalancedEntries
                });
            }
            catch (Exception ex)
            {
                return ServiceResult<JournalBalanceCheckResult>.Failure($"خطأ في فحص توازن القيود: {ex.Message}");
            }
        }

        /// <inheritdoc />
        public async Task<ServiceResult<InventoryCheckResult>> CheckInventoryReconciliationAsync(CancellationToken ct = default)
        {
            try
            {
                // Incoming movement types (increase stock)
                var incomingTypes = new[]
                {
                    MovementType.PurchaseIn,
                    MovementType.SalesReturn,
                    MovementType.AdjustmentIn,
                    MovementType.TransferIn,
                    MovementType.OpeningBalance
                };

                // Calculate expected quantity per product+warehouse from movements
                var movementSums = await _context.InventoryMovements
                    .GroupBy(m => new { m.ProductId, m.WarehouseId })
                    .Select(g => new
                    {
                        g.Key.ProductId,
                        g.Key.WarehouseId,
                        ExpectedQuantity =
                            g.Where(m => incomingTypes.Contains(m.MovementType)).Sum(m => m.QuantityInBaseUnit)
                          - g.Where(m => !incomingTypes.Contains(m.MovementType)).Sum(m => m.QuantityInBaseUnit)
                    })
                    .ToListAsync(ct);

                // Current warehouse product quantities
                var warehouseProducts = await _context.WarehouseProducts
                    .Select(wp => new
                    {
                        wp.ProductId,
                        wp.WarehouseId,
                        wp.Quantity
                    })
                    .ToListAsync(ct);

                // Build lookup from movements
                var movementLookup = movementSums
                    .ToDictionary(m => (m.ProductId, m.WarehouseId), m => m.ExpectedQuantity);

                // Load product and warehouse names for reporting
                var productIds = warehouseProducts.Select(wp => wp.ProductId).Distinct().ToList();
                var warehouseIds = warehouseProducts.Select(wp => wp.WarehouseId).Distinct().ToList();

                var products = await _context.Products
                    .Where(p => productIds.Contains(p.Id))
                    .Select(p => new { p.Id, p.Code, p.NameAr })
                    .ToDictionaryAsync(p => p.Id, ct);

                var warehouses = await _context.Warehouses
                    .Where(w => warehouseIds.Contains(w.Id))
                    .Select(w => new { w.Id, w.NameAr })
                    .ToDictionaryAsync(w => w.Id, ct);

                var inconsistencies = new List<InventoryInconsistencyDto>();

                foreach (var wp in warehouseProducts)
                {
                    var key = (wp.ProductId, wp.WarehouseId);
                    var expected = movementLookup.ContainsKey(key) ? movementLookup[key] : 0m;

                    if (expected != wp.Quantity)
                    {
                        products.TryGetValue(wp.ProductId, out var prod);
                        warehouses.TryGetValue(wp.WarehouseId, out var wh);

                        inconsistencies.Add(new InventoryInconsistencyDto
                        {
                            ProductId = wp.ProductId,
                            ProductCode = prod?.Code ?? "—",
                            ProductName = prod?.NameAr ?? "—",
                            WarehouseId = wp.WarehouseId,
                            WarehouseName = wh?.NameAr ?? "—",
                            ExpectedQuantity = expected,
                            ActualQuantity = wp.Quantity,
                            Difference = expected - wp.Quantity
                        });
                    }
                }

                return ServiceResult<InventoryCheckResult>.Success(new InventoryCheckResult
                {
                    IsConsistent = inconsistencies.Count == 0,
                    TotalProductsChecked = warehouseProducts.Count,
                    InconsistentCount = inconsistencies.Count,
                    Inconsistencies = inconsistencies
                });
            }
            catch (Exception ex)
            {
                return ServiceResult<InventoryCheckResult>.Failure($"خطأ في فحص تطابق المخزون: {ex.Message}");
            }
        }

        /// <inheritdoc />
        public async Task<ServiceResult<IntegrityReportDto>> RunFullCheckAsync(CancellationToken ct = default)
        {
            try
            {
                var trialBalanceResult = await CheckTrialBalanceAsync(ct);
                var journalBalanceResult = await CheckJournalBalancesAsync(ct);
                var inventoryResult = await CheckInventoryReconciliationAsync(ct);

                // If any sub-check failed at the service level, report the first error
                if (trialBalanceResult.IsFailure)
                    return ServiceResult<IntegrityReportDto>.Failure(trialBalanceResult.ErrorMessage);

                if (journalBalanceResult.IsFailure)
                    return ServiceResult<IntegrityReportDto>.Failure(journalBalanceResult.ErrorMessage);

                if (inventoryResult.IsFailure)
                    return ServiceResult<IntegrityReportDto>.Failure(inventoryResult.ErrorMessage);

                var overallHealthy = trialBalanceResult.Data.IsBalanced
                                  && journalBalanceResult.Data.AllBalanced
                                  && inventoryResult.Data.IsConsistent;

                return ServiceResult<IntegrityReportDto>.Success(new IntegrityReportDto
                {
                    CheckDate = _dateTime.UtcNow,
                    TrialBalance = trialBalanceResult.Data,
                    JournalBalance = journalBalanceResult.Data,
                    Inventory = inventoryResult.Data,
                    OverallHealthy = overallHealthy
                });
            }
            catch (Exception ex)
            {
                return ServiceResult<IntegrityReportDto>.Failure($"خطأ في فحص سلامة البيانات: {ex.Message}");
            }
        }
    }
}

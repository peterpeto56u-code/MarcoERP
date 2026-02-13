using System;
using System.Collections.Generic;

namespace MarcoERP.Application.DTOs.Settings
{
    // ── Trial Balance ────────────────────────────────────────

    /// <summary>
    /// Result of the trial balance integrity check (ميزان المراجعة).
    /// </summary>
    public sealed class TrialBalanceCheckResult
    {
        /// <summary>True if total debits == total credits across all posted entries.</summary>
        public bool IsBalanced { get; set; }

        /// <summary>Grand total of debit amounts.</summary>
        public decimal TotalDebits { get; set; }

        /// <summary>Grand total of credit amounts.</summary>
        public decimal TotalCredits { get; set; }

        /// <summary>Absolute difference (TotalDebits − TotalCredits).</summary>
        public decimal Difference { get; set; }

        /// <summary>Accounts where per-account DR ≠ expected balance (empty when balanced).</summary>
        public List<UnbalancedAccountDto> UnbalancedAccounts { get; set; } = new();
    }

    /// <summary>
    /// An account whose debit/credit totals are part of the imbalance detail.
    /// </summary>
    public sealed class UnbalancedAccountDto
    {
        public int AccountId { get; set; }
        public string AccountCode { get; set; }
        public string AccountName { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
    }

    // ── Journal Balance ──────────────────────────────────────

    /// <summary>
    /// Result of the per-entry journal balance check (توازن القيود).
    /// </summary>
    public sealed class JournalBalanceCheckResult
    {
        /// <summary>True if every posted journal entry is balanced.</summary>
        public bool AllBalanced { get; set; }

        /// <summary>Number of posted journal entries checked.</summary>
        public int TotalChecked { get; set; }

        /// <summary>Number of unbalanced entries found.</summary>
        public int UnbalancedCount { get; set; }

        /// <summary>Details of each unbalanced entry.</summary>
        public List<UnbalancedJournalEntryDto> UnbalancedEntries { get; set; } = new();
    }

    /// <summary>
    /// A posted journal entry whose line debits ≠ line credits.
    /// </summary>
    public sealed class UnbalancedJournalEntryDto
    {
        public int JournalEntryId { get; set; }
        public string JournalNumber { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal Difference { get; set; }
    }

    // ── Inventory Reconciliation ─────────────────────────────

    /// <summary>
    /// Result of the inventory reconciliation check (تطابق المخزون).
    /// </summary>
    public sealed class InventoryCheckResult
    {
        /// <summary>True if all warehouse product quantities match movement sums.</summary>
        public bool IsConsistent { get; set; }

        /// <summary>Number of product-warehouse combinations checked.</summary>
        public int TotalProductsChecked { get; set; }

        /// <summary>Number of inconsistencies found.</summary>
        public int InconsistentCount { get; set; }

        /// <summary>Details of each inconsistency.</summary>
        public List<InventoryInconsistencyDto> Inconsistencies { get; set; } = new();
    }

    /// <summary>
    /// A product-warehouse record whose stored quantity ≠ calculated movement sum.
    /// </summary>
    public sealed class InventoryInconsistencyDto
    {
        public int ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        /// <summary>Quantity derived from summing all inventory movements.</summary>
        public decimal ExpectedQuantity { get; set; }
        /// <summary>Current quantity stored in WarehouseProduct record.</summary>
        public decimal ActualQuantity { get; set; }
        /// <summary>ExpectedQuantity − ActualQuantity.</summary>
        public decimal Difference { get; set; }
    }

    // ── Combined Report ──────────────────────────────────────

    /// <summary>
    /// Combined integrity report (تقرير سلامة البيانات الشامل).
    /// </summary>
    public sealed class IntegrityReportDto
    {
        /// <summary>Timestamp when the check was performed.</summary>
        public DateTime CheckDate { get; set; }

        /// <summary>Trial balance check result.</summary>
        public TrialBalanceCheckResult TrialBalance { get; set; }

        /// <summary>Journal balance check result.</summary>
        public JournalBalanceCheckResult JournalBalance { get; set; }

        /// <summary>Inventory reconciliation result.</summary>
        public InventoryCheckResult Inventory { get; set; }

        /// <summary>True only if all three checks pass.</summary>
        public bool OverallHealthy { get; set; }
    }
}

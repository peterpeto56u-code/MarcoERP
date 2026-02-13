using System;
using System.Collections.Generic;

namespace MarcoERP.Application.DTOs.Treasury
{
    // ════════════════════════════════════════════════════════════
    //  Bank Reconciliation DTOs
    // ════════════════════════════════════════════════════════════

    /// <summary>Full bank reconciliation details.</summary>
    public sealed class BankReconciliationDto
    {
        public int Id { get; set; }
        public int BankAccountId { get; set; }
        public string BankAccountName { get; set; }
        public DateTime ReconciliationDate { get; set; }
        public decimal StatementBalance { get; set; }
        public decimal SystemBalance { get; set; }
        public decimal Difference { get; set; }
        public bool IsCompleted { get; set; }
        public string Notes { get; set; }
        public List<BankReconciliationItemDto> Items { get; set; } = new();
    }

    /// <summary>DTO for creating a new bank reconciliation.</summary>
    public sealed class CreateBankReconciliationDto
    {
        public int BankAccountId { get; set; }
        public DateTime ReconciliationDate { get; set; }
        public decimal StatementBalance { get; set; }
        public string Notes { get; set; }
    }

    /// <summary>DTO for updating a bank reconciliation.</summary>
    public sealed class UpdateBankReconciliationDto
    {
        public int Id { get; set; }
        public DateTime ReconciliationDate { get; set; }
        public decimal StatementBalance { get; set; }
        public string Notes { get; set; }
    }

    /// <summary>DTO for a reconciliation line item.</summary>
    public sealed class BankReconciliationItemDto
    {
        public int Id { get; set; }
        public int BankReconciliationId { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public string Reference { get; set; }
        public bool IsMatched { get; set; }
        public int? JournalEntryId { get; set; }
    }

    /// <summary>DTO for adding/updating a reconciliation item.</summary>
    public sealed class CreateBankReconciliationItemDto
    {
        public int BankReconciliationId { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public string Reference { get; set; }
    }
}

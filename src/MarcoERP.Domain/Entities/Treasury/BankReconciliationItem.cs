using System;
using MarcoERP.Domain.Entities.Common;
using MarcoERP.Domain.Exceptions.Treasury;

namespace MarcoERP.Domain.Entities.Treasury
{
    /// <summary>
    /// A single line item in a bank reconciliation (بند تسوية بنكية).
    /// Represents an adjustment between the bank statement and system records.
    /// </summary>
    public sealed class BankReconciliationItem : AuditableEntity
    {
        /// <summary>EF Core only.</summary>
        private BankReconciliationItem() { }

        /// <summary>
        /// Creates a new reconciliation item.
        /// </summary>
        public BankReconciliationItem(
            int bankReconciliationId,
            DateTime transactionDate,
            string description,
            decimal amount,
            string reference = null)
        {
            if (string.IsNullOrWhiteSpace(description))
                throw new TreasuryDomainException("وصف البند مطلوب.");

            BankReconciliationId = bankReconciliationId;
            TransactionDate = transactionDate;
            Description = description.Trim();
            Amount = amount;
            Reference = reference?.Trim();
            IsMatched = false;
        }

        // ── Properties ───────────────────────────────────────────

        /// <summary>FK to BankReconciliation.</summary>
        public int BankReconciliationId { get; private set; }

        /// <summary>Transaction date from bank statement.</summary>
        public DateTime TransactionDate { get; private set; }

        /// <summary>Description of the transaction.</summary>
        public string Description { get; private set; }

        /// <summary>Amount (positive = debit from bank, negative = credit to bank).</summary>
        public decimal Amount { get; private set; }

        /// <summary>External reference number.</summary>
        public string Reference { get; private set; }

        /// <summary>Whether this item has been matched to a system transaction.</summary>
        public bool IsMatched { get; private set; }

        /// <summary>Optional linked journal entry ID for matching.</summary>
        public int? JournalEntryId { get; private set; }

        /// <summary>Navigation property.</summary>
        public BankReconciliation BankReconciliation { get; private set; }

        // ── Domain Methods ───────────────────────────────────────

        /// <summary>Updates the reconciliation item.</summary>
        public void Update(DateTime transactionDate, string description, decimal amount, string reference)
        {
            if (string.IsNullOrWhiteSpace(description))
                throw new TreasuryDomainException("وصف البند مطلوب.");

            TransactionDate = transactionDate;
            Description = description.Trim();
            Amount = amount;
            Reference = reference?.Trim();
        }

        /// <summary>Marks this item as matched to a system transaction.</summary>
        public void Match(int? journalEntryId = null)
        {
            IsMatched = true;
            JournalEntryId = journalEntryId;
        }

        /// <summary>Unmatches this item.</summary>
        public void Unmatch()
        {
            IsMatched = false;
            JournalEntryId = null;
        }
    }
}

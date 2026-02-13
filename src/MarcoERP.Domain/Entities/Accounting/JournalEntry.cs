using System;
using System.Collections.Generic;
using System.Linq;
using MarcoERP.Domain.Entities.Common;
using MarcoERP.Domain.Enums;
using MarcoERP.Domain.Entities.Accounting.Events;
using MarcoERP.Domain.Entities.Accounting.Policies;
using MarcoERP.Domain.Exceptions.Accounting;

namespace MarcoERP.Domain.Entities.Accounting
{
    /// <summary>
    /// Represents a double-entry journal entry (القيد اليومي).
    /// Lifecycle: Draft → Posted → (optionally) Reversed.
    /// Posted entries are immutable. Corrections via Reversal or Adjustment only.
    /// </summary>
    public sealed class JournalEntry : CompanyAwareEntity
    {
        private readonly List<JournalEntryLine> _lines = new();
        private readonly List<IDomainEvent> _domainEvents = new();

        // ── Constructors ────────────────────────────────────────

        /// <summary>EF Core only.</summary>
        private JournalEntry() { }

        /// <summary>
        /// Internal constructor for factory methods.
        /// </summary>
        private JournalEntry(
            string draftCode,
            DateTime journalDate,
            string description,
            SourceType sourceType,
            int fiscalYearId,
            int fiscalPeriodId)
        {
            if (string.IsNullOrWhiteSpace(draftCode))
                throw new JournalEntryDomainException("كود المسودة مطلوب.");

            if (string.IsNullOrWhiteSpace(description))
                throw new JournalEntryDomainException("وصف القيد مطلوب.");   // JE-INV-13

            DraftCode = draftCode.Trim();
            JournalDate = journalDate;
            Description = description.Trim();
            SourceType = sourceType;
            FiscalYearId = fiscalYearId;
            FiscalPeriodId = fiscalPeriodId;
            Status = JournalEntryStatus.Draft;
            TotalDebit = 0m;
            TotalCredit = 0m;
        }

        // ── Properties ──────────────────────────────────────────

        /// <summary>Final sequential code assigned on posting. Null while draft.</summary>
        public string JournalNumber { get; private set; }

        /// <summary>Temporary code: DRAFT-{GUID:8}, replaced upon posting.</summary>
        public string DraftCode { get; private set; }

        /// <summary>Transaction date (must fall within open period).</summary>
        public DateTime JournalDate { get; private set; }

        /// <summary>UTC timestamp when posted. Null while draft.</summary>
        public DateTime? PostingDate { get; private set; }

        /// <summary>Narrative describing the transaction (JE-INV-13: mandatory).</summary>
        public string Description { get; private set; }

        /// <summary>External reference (invoice number, etc.).</summary>
        public string ReferenceNumber { get; private set; }

        /// <summary>Draft = 0, Posted = 1, Reversed = 2.</summary>
        public JournalEntryStatus Status { get; private set; }

        /// <summary>Source of this journal entry.</summary>
        public SourceType SourceType { get; private set; }

        /// <summary>FK to originating document (null for manual entries).</summary>
        public int? SourceId { get; private set; }

        /// <summary>FK to FiscalYear.</summary>
        public int FiscalYearId { get; private set; }

        /// <summary>FK to FiscalPeriod.</summary>
        public int FiscalPeriodId { get; private set; }

        /// <summary>Optional cost center tag.</summary>
        public int? CostCenterId { get; private set; }

        /// <summary>FK to original entry if this IS a reversal.</summary>
        public int? ReversedEntryId { get; private set; }

        /// <summary>FK to reversal entry if this WAS reversed.</summary>
        public int? ReversalEntryId { get; private set; }

        /// <summary>FK to original entry if this is an adjustment.</summary>
        public int? AdjustedEntryId { get; private set; }

        /// <summary>Mandatory reason when this is a reversal.</summary>
        public string ReversalReason { get; private set; }

        /// <summary>User who posted.</summary>
        public string PostedBy { get; private set; }

        /// <summary>Calculated: SUM of all line debit amounts.</summary>
        public decimal TotalDebit { get; private set; }

        /// <summary>Calculated: SUM of all line credit amounts.</summary>
        public decimal TotalCredit { get; private set; }

        /// <summary>Navigation to journal lines.</summary>
        public IReadOnlyCollection<JournalEntryLine> Lines => _lines.AsReadOnly();

        /// <summary>Domain events raised by this entity.</summary>
        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        // ── Factory Methods ─────────────────────────────────────

        /// <summary>
        /// Creates a new draft journal entry.
        /// DraftCode is auto-generated as DRAFT-{GUID:8}.
        /// </summary>
        public static JournalEntry CreateDraft(
            DateTime journalDate,
            string description,
            SourceType sourceType,
            int fiscalYearId,
            int fiscalPeriodId,
            string referenceNumber = null,
            int? costCenterId = null,
            int? sourceId = null)
        {
            var draftCode = DomainConstants.DraftCodePrefix + Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();

            var entry = new JournalEntry(
                draftCode,
                journalDate,
                description,
                sourceType,
                fiscalYearId,
                fiscalPeriodId)
            {
                ReferenceNumber = referenceNumber?.Trim(),
                CostCenterId = costCenterId,
                SourceId = sourceId
            };

            return entry;
        }

        /// <summary>
        /// Creates a draft journal entry for opening balances.
        /// Only balance sheet accounts are allowed.
        /// </summary>
        public static JournalEntry CreateOpeningBalanceDraft(
            DateTime journalDate,
            string description,
            int fiscalYearId,
            int fiscalPeriodId)
        {
            return CreateDraft(
                journalDate,
                description,
                SourceType.Opening,
                fiscalYearId,
                fiscalPeriodId);
        }

        /// <summary>
        /// Creates a draft adjustment entry for a posted entry in a closed period.
        /// The adjustment must be created in an open period.
        /// </summary>
        public static JournalEntry CreateAdjustment(
            DateTime journalDate,
            string description,
            int fiscalYearId,
            int fiscalPeriodId,
            int adjustedEntryId)
        {
            var entry = CreateDraft(
                journalDate,
                description,
                SourceType.Adjustment,
                fiscalYearId,
                fiscalPeriodId);

            entry.AdjustedEntryId = adjustedEntryId;
            return entry;
        }

        // ── Line Management ─────────────────────────────────────

        /// <summary>
        /// Adds a line to a draft journal entry.
        /// </summary>
        public void AddLine(int accountId, decimal debitAmount, decimal creditAmount, DateTime createdAt,
            string lineDescription = null, int? costCenterId = null, int? warehouseId = null)
        {
            EnsureDraft();

            var lineNumber = _lines.Count + 1;
            var line = JournalEntryLine.Create(
                accountId,
                lineNumber,
                debitAmount,
                creditAmount,
                lineDescription,
                costCenterId,
                warehouseId,
                createdAt);
            _lines.Add(line);
            RecalculateTotals();
        }

        /// <summary>
        /// Removes a line by its line number from a draft entry.
        /// </summary>
        public void RemoveLine(int lineNumber)
        {
            EnsureDraft();

            var line = _lines.FirstOrDefault(l => l.LineNumber == lineNumber);
            if (line == null)
                throw new JournalEntryDomainException("سطر القيد غير موجود.");

            _lines.Remove(line);

            // Re-number remaining lines sequentially
            for (int i = 0; i < _lines.Count; i++)
            {
                _lines[i].SetLineNumber(i + 1);
            }

            RecalculateTotals();
        }

        /// <summary>
        /// Updates the debit/credit amounts of a specific line.
        /// </summary>
        public void UpdateLineAmount(int lineNumber, decimal debitAmount, decimal creditAmount)
        {
            EnsureDraft();

            var line = _lines.FirstOrDefault(l => l.LineNumber == lineNumber);
            if (line == null)
                throw new JournalEntryDomainException("سطر القيد غير موجود.");

            line.UpdateAmount(debitAmount, creditAmount);
            RecalculateTotals();
        }

        /// <summary>
        /// Updates the description text and metadata of a draft entry.
        /// </summary>
        public void UpdateDraft(string description, string referenceNumber = null, int? costCenterId = null)
        {
            EnsureDraft();

            if (string.IsNullOrWhiteSpace(description))
                throw new JournalEntryDomainException("وصف القيد مطلوب.");

            Description = description.Trim();
            ReferenceNumber = referenceNumber?.Trim();
            CostCenterId = costCenterId;
        }

        // ── Validation ──────────────────────────────────────────

        /// <summary>
        /// Validates the journal entry per domain invariants JE-INV-01 through JE-INV-06, JE-INV-13.
        /// Returns a list of validation failures (never throws).
        /// </summary>
        public List<string> Validate()
        {
            var errors = new List<string>();

            // JE-INV-13: Description mandatory
            if (string.IsNullOrWhiteSpace(Description))
                errors.Add("وصف القيد مطلوب.");

            // JE-INV-01: At least 2 lines
            if (_lines.Count < 2)
                errors.Add("القيد يجب أن يحتوي على سطرين على الأقل.");

            // JE-INV-02: Balanced
            RecalculateTotals();
            if (TotalDebit != TotalCredit)
                errors.Add($"القيد غير متوازن. إجمالي المدين: {TotalDebit:N2} ≠ إجمالي الدائن: {TotalCredit:N2}.");

            foreach (var line in _lines)
            {
                // JE-INV-05: No negative amounts
                if (line.DebitAmount < 0 || line.CreditAmount < 0)
                    errors.Add($"سطر {line.LineNumber}: المبالغ السالبة غير مسموحة.");

                // JE-INV-03: Not both sides
                if (line.DebitAmount > 0 && line.CreditAmount > 0)
                    errors.Add($"سطر {line.LineNumber}: لا يمكن أن يكون السطر مدين ودائن في نفس الوقت.");

                // JE-INV-04: Not both zero
                if (line.DebitAmount == 0 && line.CreditAmount == 0)
                    errors.Add($"سطر {line.LineNumber}: لا يمكن أن يكون المدين والدائن صفر.");
            }

            // JE-INV-12: Reversed entry cannot be reversed again
            if (Status == JournalEntryStatus.Reversed)
                errors.Add("لا يمكن عكس قيد معكوس بالفعل.");

            return errors;
        }

        // ── Posting ─────────────────────────────────────────────

        /// <summary>
        /// Posts the journal entry. Assigns the final journal number.
        /// JE-INV-11: After posting, no modifications allowed.
        /// Called by Application layer after all cross-aggregate validations pass.
        /// </summary>
        public void Post(string journalNumber, string postedBy, DateTime postedAt)
        {
            if (Status != JournalEntryStatus.Draft)
                throw new JournalEntryDomainException("يمكن ترحيل المسودات فقط.");

            if (string.IsNullOrWhiteSpace(journalNumber))
                throw new JournalEntryDomainException("رقم القيد النهائي مطلوب عند الترحيل.");

            if (string.IsNullOrWhiteSpace(postedBy))
                throw new JournalEntryDomainException("اسم المستخدم مطلوب عند الترحيل.");

            // Run domain-level validations
            var errors = Validate();
            if (errors.Count > 0)
                throw new JournalEntryDomainException(string.Join(" | ", errors));

            Status = JournalEntryStatus.Posted;
            JournalNumber = journalNumber.Trim();
            PostingDate = postedAt;
            PostedBy = postedBy.Trim();

            _domainEvents.Add(new JournalEntryPostedEvent(Id, postedAt));
        }

        // ── Reversal ────────────────────────────────────────────

        /// <summary>
        /// Creates a reversal entry for this posted journal entry.
        /// Swaps debit/credit on all lines.
        /// JE-INV-12: Reversed entries cannot be reversed again.
        /// </summary>
        public JournalEntry CreateReversal(
            DateTime reversalDate,
            string reversalReason,
            int reversalFiscalYearId,
            int reversalFiscalPeriodId)
        {
            if (Status != JournalEntryStatus.Posted)
                throw new JournalEntryDomainException("يمكن عكس القيود المرحّلة فقط.");

            if (ReversalEntryId.HasValue)
                throw new JournalEntryDomainException("تم عكس هذا القيد بالفعل.");

            if (string.IsNullOrWhiteSpace(reversalReason))
                throw new JournalEntryDomainException("سبب العكس مطلوب.");

            var reversalEntry = CreateDraft(
                reversalDate,
                "عكس: " + Description,
                SourceType,
                reversalFiscalYearId,
                reversalFiscalPeriodId,
                ReferenceNumber);

            reversalEntry.ReversedEntryId = Id;
            reversalEntry.ReversalReason = reversalReason.Trim();

            // Swap debit/credit on all lines
            foreach (var line in _lines)
            {
                reversalEntry.AddLine(
                    line.AccountId,
                    line.CreditAmount,  // Debit ↔ Credit swap
                    line.DebitAmount,
                    reversalDate,
                    "عكس: " + (line.Description ?? string.Empty),
                    line.CostCenterId,
                    line.WarehouseId);
            }

            return reversalEntry;
        }

        /// <summary>
        /// Marks this entry as reversed and stores the reference to the reversal entry.
        /// Called after the reversal entry is successfully posted.
        /// </summary>
        public void MarkAsReversed(int reversalEntryId)
        {
            if (Status != JournalEntryStatus.Posted)
                throw new JournalEntryDomainException("يمكن عكس القيود المرحّلة فقط.");

            ReversalEntryId = reversalEntryId;
            Status = JournalEntryStatus.Reversed;
        }

        // ── Soft Delete Override ────────────────────────────────

        /// <summary>
        /// Only draft entries can be soft-deleted.
        /// Posted/Reversed entries are immutable.
        /// </summary>
        public override void SoftDelete(string deletedBy, DateTime deletedAt)
        {
            if (Status != JournalEntryStatus.Draft)
                throw new JournalEntryDomainException("لا يمكن حذف القيود المرحّلة — استخدم العكس أو التعديل.");

            base.SoftDelete(deletedBy, deletedAt);
        }

        // ── Domain Events ───────────────────────────────────────

        /// <summary>Clears collected domain events after dispatch.</summary>
        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }

        // ── Private Helpers ─────────────────────────────────────

        private void EnsureDraft()
        {
            if (Status != JournalEntryStatus.Draft)
                throw new JournalEntryDomainException("لا يمكن تعديل القيد بعد الترحيل.");
        }

        private void RecalculateTotals()
        {
            TotalDebit = _lines.Sum(l => l.DebitAmount);
            TotalCredit = _lines.Sum(l => l.CreditAmount);
        }
    }
}

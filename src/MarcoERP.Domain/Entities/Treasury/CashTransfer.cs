using System;
using MarcoERP.Domain.Entities.Common;
using MarcoERP.Domain.Enums;
using MarcoERP.Domain.Exceptions.Treasury;

namespace MarcoERP.Domain.Entities.Treasury
{
    /// <summary>
    /// Represents a cash transfer between two cashboxes (تحويل بين الخزن).
    /// Lifecycle: Draft → Posted → Cancelled.
    /// On posting: auto-generates journal entry (DR Target Cashbox / CR Source Cashbox).
    /// </summary>
    public sealed class CashTransfer : CompanyAwareEntity
    {
        // ── Constructors ─────────────────────────────────────────

        /// <summary>EF Core only.</summary>
        private CashTransfer() { }

        /// <summary>
        /// Creates a new cash transfer in Draft status.
        /// </summary>
        public CashTransfer(
            string transferNumber,
            DateTime transferDate,
            int sourceCashboxId,
            int targetCashboxId,
            decimal amount,
            string description,
            string notes = null)
        {
            if (string.IsNullOrWhiteSpace(transferNumber))
                throw new TreasuryDomainException("رقم التحويل مطلوب.");
            if (sourceCashboxId <= 0)
                throw new TreasuryDomainException("خزنة المصدر مطلوبة.");
            if (targetCashboxId <= 0)
                throw new TreasuryDomainException("خزنة الاستلام مطلوبة.");
            if (sourceCashboxId == targetCashboxId)
                throw new TreasuryDomainException("لا يمكن التحويل من وإلى نفس الخزنة.");
            if (amount <= 0)
                throw new TreasuryDomainException("مبلغ التحويل يجب أن يكون أكبر من صفر.");
            if (string.IsNullOrWhiteSpace(description))
                throw new TreasuryDomainException("وصف التحويل مطلوب.");

            TransferNumber = transferNumber.Trim();
            TransferDate = transferDate;
            SourceCashboxId = sourceCashboxId;
            TargetCashboxId = targetCashboxId;
            Amount = amount;
            Description = description.Trim();
            Notes = notes?.Trim();
            Status = InvoiceStatus.Draft;
            SourceCashbox = null;
            TargetCashbox = null;
        }

        // ── Properties ───────────────────────────────────────────

        /// <summary>Auto-generated transfer number (CT-YYYYMM-####).</summary>
        public string TransferNumber { get; private set; }

        /// <summary>Transfer date.</summary>
        public DateTime TransferDate { get; private set; }

        /// <summary>FK to source Cashbox (money leaves here).</summary>
        public int SourceCashboxId { get; private set; }

        /// <summary>Navigation to source Cashbox.</summary>
        public Cashbox SourceCashbox { get; private set; }

        /// <summary>FK to target Cashbox (money arrives here).</summary>
        public int TargetCashboxId { get; private set; }

        /// <summary>Navigation to target Cashbox.</summary>
        public Cashbox TargetCashbox { get; private set; }

        /// <summary>Transfer amount.</summary>
        public decimal Amount { get; private set; }

        /// <summary>Required description of the transfer.</summary>
        public string Description { get; private set; }

        /// <summary>Optional notes.</summary>
        public string Notes { get; private set; }

        /// <summary>Document status: Draft → Posted → Cancelled.</summary>
        public InvoiceStatus Status { get; private set; }

        /// <summary>FK to the auto-generated journal entry (set on posting).</summary>
        public int? JournalEntryId { get; private set; }

        // ── Domain Methods ───────────────────────────────────────

        /// <summary>
        /// Updates the transfer header. Only allowed while Draft.
        /// </summary>
        public void UpdateHeader(
            DateTime transferDate,
            int sourceCashboxId,
            int targetCashboxId,
            decimal amount,
            string description,
            string notes)
        {
            EnsureDraft("لا يمكن تعديل تحويل مرحّل أو ملغى.");

            if (sourceCashboxId <= 0)
                throw new TreasuryDomainException("خزنة المصدر مطلوبة.");
            if (targetCashboxId <= 0)
                throw new TreasuryDomainException("خزنة الاستلام مطلوبة.");
            if (sourceCashboxId == targetCashboxId)
                throw new TreasuryDomainException("لا يمكن التحويل من وإلى نفس الخزنة.");
            if (amount <= 0)
                throw new TreasuryDomainException("مبلغ التحويل يجب أن يكون أكبر من صفر.");
            if (string.IsNullOrWhiteSpace(description))
                throw new TreasuryDomainException("وصف التحويل مطلوب.");

            TransferDate = transferDate;
            SourceCashboxId = sourceCashboxId;
            TargetCashboxId = targetCashboxId;
            Amount = amount;
            Description = description.Trim();
            Notes = notes?.Trim();
        }

        /// <summary>
        /// Posts the cash transfer. Journal entry ID is assigned.
        /// </summary>
        public void Post(int journalEntryId)
        {
            EnsureDraft("لا يمكن ترحيل تحويل مرحّل بالفعل أو ملغى.");

            if (journalEntryId <= 0)
                throw new TreasuryDomainException("معرف القيد المحاسبي غير صالح.");

            Status = InvoiceStatus.Posted;
            JournalEntryId = journalEntryId;
        }

        /// <summary>
        /// Cancels a posted cash transfer.
        /// </summary>
        public void Cancel()
        {
            if (Status != InvoiceStatus.Posted)
                throw new TreasuryDomainException("لا يمكن إلغاء إلا التحويلات المرحّلة.");
            Status = InvoiceStatus.Cancelled;
        }

        // ── Soft Delete Override ────────────────────────────────

        /// <summary>
        /// Only draft transfers can be soft-deleted.
        /// Posted/Cancelled transfers are immutable.
        /// </summary>
        public override void SoftDelete(string deletedBy, DateTime deletedAt)
        {
            if (Status != InvoiceStatus.Draft)
                throw new TreasuryDomainException("لا يمكن حذف تحويل مرحّل أو ملغى — استخدم الإلغاء.");

            base.SoftDelete(deletedBy, deletedAt);
        }

        // ── Private Helpers ──────────────────────────────────────

        private void EnsureDraft(string errorMessage)
        {
            if (Status != InvoiceStatus.Draft)
                throw new TreasuryDomainException(errorMessage);
        }
    }
}

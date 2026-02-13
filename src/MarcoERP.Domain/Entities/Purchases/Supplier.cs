using System;
using MarcoERP.Domain.Entities.Accounting;
using MarcoERP.Domain.Entities.Common;
using MarcoERP.Domain.Exceptions.Purchases;

namespace MarcoERP.Domain.Entities.Purchases
{
    /// <summary>
    /// Represents a supplier (مورد) in the purchases sub-ledger.
    /// Uses sub-ledger pattern: individual balance tracked via invoices/payments,
    /// GL control account = 2111 (الدائنون — ذمم تجارية).
    /// </summary>
    public sealed class Supplier : CompanyAwareEntity
    {
        // ── Constructors ────────────────────────────────────────

        /// <summary>EF Core only.</summary>
        private Supplier() { }

        /// <summary>
        /// Creates a new Supplier with full invariant validation.
        /// </summary>
        public Supplier(SupplierDraft draft)
        {
            if (draft == null)
                throw new ArgumentNullException(nameof(draft));

            if (string.IsNullOrWhiteSpace(draft.Code))
                throw new SupplierDomainException("كود المورد مطلوب.");

            if (string.IsNullOrWhiteSpace(draft.NameAr))
                throw new SupplierDomainException("اسم المورد بالعربي مطلوب.");

            Code = draft.Code.Trim();
            NameAr = draft.NameAr.Trim();
            NameEn = draft.NameEn?.Trim();
            Phone = draft.Phone?.Trim();
            Mobile = draft.Mobile?.Trim();
            Address = draft.Address?.Trim();
            City = draft.City?.Trim();
            TaxNumber = draft.TaxNumber?.Trim();
            PreviousBalance = draft.PreviousBalance;
            Notes = draft.Notes?.Trim();
            IsActive = true;
            Account = null;
        }

        // ── Properties ──────────────────────────────────────────

        /// <summary>Unique supplier code (auto-generated or user-set).</summary>
        public string Code { get; private set; }

        /// <summary>Arabic name (required).</summary>
        public string NameAr { get; private set; }

        /// <summary>English name (optional).</summary>
        public string NameEn { get; private set; }

        /// <summary>Phone number.</summary>
        public string Phone { get; private set; }

        /// <summary>Mobile number.</summary>
        public string Mobile { get; private set; }

        /// <summary>Full address.</summary>
        public string Address { get; private set; }

        /// <summary>City / Governorate.</summary>
        public string City { get; private set; }

        /// <summary>Tax registration number (الرقم الضريبي).</summary>
        public string TaxNumber { get; private set; }

        /// <summary>
        /// Opening balance brought forward from a previous system.
        /// Positive = we owe the supplier (credit). Negative = advance payment (debit).
        /// Set once at creation; adjustments done via journal entries.
        /// </summary>
        public decimal PreviousBalance { get; private set; }

        /// <summary>Active flag — inactive suppliers cannot appear on new purchase orders.</summary>
        public bool IsActive { get; private set; }

        /// <summary>Free-form notes.</summary>
        public string Notes { get; private set; }

        /// <summary>
        /// FK to the GL control account (Accounts Payable — 2111).
        /// Linked automatically on creation.
        /// </summary>
        public int? AccountId { get; private set; }

        /// <summary>Navigation property to the linked GL account.</summary>
        public Account Account { get; private set; }

        // ── Domain Methods ──────────────────────────────────────

        /// <summary>Updates supplier information.</summary>
        public void Update(SupplierUpdate update)
        {
            if (update == null)
                throw new ArgumentNullException(nameof(update));

            if (string.IsNullOrWhiteSpace(update.NameAr))
                throw new SupplierDomainException("اسم المورد بالعربي مطلوب.");

            NameAr = update.NameAr.Trim();
            NameEn = update.NameEn?.Trim();
            Phone = update.Phone?.Trim();
            Mobile = update.Mobile?.Trim();
            Address = update.Address?.Trim();
            City = update.City?.Trim();
            TaxNumber = update.TaxNumber?.Trim();
            Notes = update.Notes?.Trim();
        }

        /// <summary>Deactivates the supplier. Cannot appear on new purchase orders.</summary>
        public void Deactivate()
        {
            IsActive = false;
        }

        /// <summary>Reactivates a previously deactivated supplier.</summary>
        public void Activate()
        {
            IsActive = true;
        }

        /// <summary>
        /// Links the supplier to a GL control account (Accounts Payable).
        /// </summary>
        public void SetAccountId(int accountId)
        {
            if (accountId <= 0)
                throw new SupplierDomainException("معرف الحساب غير صالح.");

            AccountId = accountId;
        }

        /// <summary>
        /// Adjusts the previous (opening) balance. Use only for corrections.
        /// Normal balance changes happen through invoices/payments.
        /// </summary>
        public void AdjustPreviousBalance(decimal newBalance)
        {
            PreviousBalance = newBalance;
        }

        /// <summary>
        /// Override soft delete — the service layer should verify no unpaid invoices
        /// before calling this.
        /// </summary>
        public override void SoftDelete(string deletedBy, DateTime deletedAt)
        {
            base.SoftDelete(deletedBy, deletedAt);
        }
    }
    public sealed class SupplierDraft
    {
        public string Code { get; init; }
        public string NameAr { get; init; }
        public string NameEn { get; init; }
        public string Phone { get; init; }
        public string Mobile { get; init; }
        public string Address { get; init; }
        public string City { get; init; }
        public string TaxNumber { get; init; }
        public decimal PreviousBalance { get; init; }
        public string Notes { get; init; }
    }

    public sealed class SupplierUpdate
    {
        public string NameAr { get; init; }
        public string NameEn { get; init; }
        public string Phone { get; init; }
        public string Mobile { get; init; }
        public string Address { get; init; }
        public string City { get; init; }
        public string TaxNumber { get; init; }
        public string Notes { get; init; }
    }
}

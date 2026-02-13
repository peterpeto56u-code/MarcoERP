using System;
using MarcoERP.Domain.Entities.Common;
using MarcoERP.Domain.Enums;
using MarcoERP.Domain.Exceptions.Sales;

namespace MarcoERP.Domain.Entities.Sales
{
    /// <summary>
    /// Represents a sales representative (مندوب مبيعات).
    /// Tracks commission rate and links to customers and invoices.
    /// </summary>
    public sealed class SalesRepresentative : SoftDeletableEntity
    {
        // ── Constructors ────────────────────────────────────────

        /// <summary>EF Core only.</summary>
        private SalesRepresentative() { }

        /// <summary>
        /// Creates a new SalesRepresentative with full invariant validation.
        /// </summary>
        public SalesRepresentative(SalesRepresentativeDraft draft)
        {
            if (draft == null)
                throw new ArgumentNullException(nameof(draft));

            if (string.IsNullOrWhiteSpace(draft.Code))
                throw new SalesRepresentativeDomainException("كود المندوب مطلوب.");

            if (string.IsNullOrWhiteSpace(draft.NameAr))
                throw new SalesRepresentativeDomainException("اسم المندوب بالعربي مطلوب.");

            if (draft.CommissionRate < 0 || draft.CommissionRate > 100)
                throw new SalesRepresentativeDomainException("نسبة العمولة يجب أن تكون بين 0 و 100.");

            Code = draft.Code.Trim();
            NameAr = draft.NameAr.Trim();
            NameEn = draft.NameEn?.Trim();
            Phone = draft.Phone?.Trim();
            Mobile = draft.Mobile?.Trim();
            Email = draft.Email?.Trim();
            CommissionRate = draft.CommissionRate;
            CommissionBasedOn = draft.CommissionBasedOn;
            Notes = draft.Notes?.Trim();
            IsActive = true;
        }

        // ── Properties ──────────────────────────────────────────

        /// <summary>Unique representative code (auto-generated or user-set).</summary>
        public string Code { get; private set; }

        /// <summary>Arabic name (required).</summary>
        public string NameAr { get; private set; }

        /// <summary>English name (optional).</summary>
        public string NameEn { get; private set; }

        /// <summary>Phone number.</summary>
        public string Phone { get; private set; }

        /// <summary>Mobile number.</summary>
        public string Mobile { get; private set; }

        /// <summary>Email address.</summary>
        public string Email { get; private set; }

        /// <summary>
        /// Default commission rate percentage (0–100).
        /// Applied on invoice net total to calculate commission.
        /// </summary>
        public decimal CommissionRate { get; private set; }

        /// <summary>
        /// Determines if commission is calculated on net sales or gross profit.
        /// Default: Sales.
        /// </summary>
        public CommissionBasis CommissionBasedOn { get; private set; }

        /// <summary>Active flag — inactive reps cannot appear on new invoices.</summary>
        public bool IsActive { get; private set; }

        /// <summary>Free-form notes.</summary>
        public string Notes { get; private set; }

        // ── Domain Methods ──────────────────────────────────────

        /// <summary>Updates representative information.</summary>
        public void Update(SalesRepresentativeUpdate update)
        {
            if (update == null)
                throw new ArgumentNullException(nameof(update));

            if (string.IsNullOrWhiteSpace(update.NameAr))
                throw new SalesRepresentativeDomainException("اسم المندوب بالعربي مطلوب.");

            if (update.CommissionRate < 0 || update.CommissionRate > 100)
                throw new SalesRepresentativeDomainException("نسبة العمولة يجب أن تكون بين 0 و 100.");

            NameAr = update.NameAr.Trim();
            NameEn = update.NameEn?.Trim();
            Phone = update.Phone?.Trim();
            Mobile = update.Mobile?.Trim();
            Email = update.Email?.Trim();
            CommissionRate = update.CommissionRate;
            CommissionBasedOn = update.CommissionBasedOn;
            Notes = update.Notes?.Trim();
        }

        /// <summary>Deactivates the representative.</summary>
        public void Deactivate()
        {
            IsActive = false;
        }

        /// <summary>Reactivates a previously deactivated representative.</summary>
        public void Activate()
        {
            IsActive = true;
        }

        /// <summary>
        /// Override soft delete.
        /// </summary>
        public override void SoftDelete(string deletedBy, DateTime deletedAt)
        {
            base.SoftDelete(deletedBy, deletedAt);
        }

        // ── Inner Draft / Update Classes ────────────────────────

        public sealed class SalesRepresentativeDraft
        {
            public string Code { get; init; }
            public string NameAr { get; init; }
            public string NameEn { get; init; }
            public string Phone { get; init; }
            public string Mobile { get; init; }
            public string Email { get; init; }
            public decimal CommissionRate { get; init; }
            public CommissionBasis CommissionBasedOn { get; init; }
            public string Notes { get; init; }
        }

        public sealed class SalesRepresentativeUpdate
        {
            public string NameAr { get; init; }
            public string NameEn { get; init; }
            public string Phone { get; init; }
            public string Mobile { get; init; }
            public string Email { get; init; }
            public decimal CommissionRate { get; init; }
            public CommissionBasis CommissionBasedOn { get; init; }
            public string Notes { get; init; }
        }
    }
}

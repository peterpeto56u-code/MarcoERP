using System;
using MarcoERP.Domain.Entities.Common;
using MarcoERP.Domain.Exceptions.Treasury;

namespace MarcoERP.Domain.Entities.Treasury
{
    /// <summary>
    /// Represents a physical or virtual cashbox (خزنة) or bank account.
    /// Each cashbox is linked to a GL leaf account under 1110 (Cash and Banks).
    /// Simple master-data entity: CRUD with activate/deactivate.
    /// </summary>
    public sealed class Cashbox : CompanyAwareEntity
    {
        // ── Constructors ─────────────────────────────────────────

        /// <summary>EF Core only.</summary>
        private Cashbox() { }

        /// <summary>
        /// Creates a new Cashbox with full invariant validation.
        /// </summary>
        public Cashbox(
            string code,
            string nameAr,
            string nameEn,
            int? accountId = null)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new TreasuryDomainException("كود الخزنة مطلوب.");

            if (string.IsNullOrWhiteSpace(nameAr))
                throw new TreasuryDomainException("اسم الخزنة بالعربي مطلوب.");

            Code = code.Trim();
            NameAr = nameAr.Trim();
            NameEn = nameEn?.Trim();
            AccountId = accountId;
            IsActive = true;
            IsDefault = false;
        }

        // ── Properties ───────────────────────────────────────────

        /// <summary>Unique cashbox code (auto-generated CBX-####).</summary>
        public string Code { get; private set; }

        /// <summary>Arabic cashbox name.</summary>
        public string NameAr { get; private set; }

        /// <summary>English cashbox name (optional).</summary>
        public string NameEn { get; private set; }

        /// <summary>FK to GL Account under 1110 Cash and Banks (e.g. 1111 Main Cash).</summary>
        public int? AccountId { get; private set; }

        /// <summary>Whether this cashbox is active and can receive transactions.</summary>
        public bool IsActive { get; private set; }

        /// <summary>Whether this is the default cashbox for new transactions.</summary>
        public bool IsDefault { get; private set; }

        // ── Domain Methods ───────────────────────────────────────

        /// <summary>Updates cashbox information.</summary>
        public void Update(string nameAr, string nameEn, int? accountId)
        {
            if (string.IsNullOrWhiteSpace(nameAr))
                throw new TreasuryDomainException("اسم الخزنة بالعربي مطلوب.");

            NameAr = nameAr.Trim();
            NameEn = nameEn?.Trim();
            AccountId = accountId;
        }

        /// <summary>Sets this cashbox as the default.</summary>
        public void SetAsDefault() => IsDefault = true;

        /// <summary>Removes default status.</summary>
        public void ClearDefault() => IsDefault = false;

        /// <summary>Deactivates the cashbox. Default cashbox cannot be deactivated.</summary>
        public void Deactivate()
        {
            if (IsDefault)
                throw new TreasuryDomainException("لا يمكن تعطيل الخزنة الافتراضية.");
            IsActive = false;
        }

        /// <summary>Activates the cashbox.</summary>
        public void Activate() => IsActive = true;
    }
}

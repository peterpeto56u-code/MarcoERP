using System;
using MarcoERP.Domain.Entities.Common;
using MarcoERP.Domain.Exceptions.Inventory;

namespace MarcoERP.Domain.Entities.Inventory
{
    /// <summary>
    /// Represents a physical or logical warehouse/store.
    /// Each warehouse has a corresponding GL account for inventory valuation.
    /// </summary>
    public sealed class Warehouse : CompanyAwareEntity
    {
        // ── Constructors ─────────────────────────────────────────

        /// <summary>EF Core only.</summary>
        private Warehouse() { }

        /// <summary>
        /// Creates a new Warehouse with full invariant validation.
        /// </summary>
        public Warehouse(
            string code,
            string nameAr,
            string nameEn,
            string address,
            string phone,
            int? accountId = null)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new InventoryDomainException("كود المخزن مطلوب.");

            if (string.IsNullOrWhiteSpace(nameAr))
                throw new InventoryDomainException("اسم المخزن بالعربي مطلوب.");

            Code = code.Trim();
            NameAr = nameAr.Trim();
            NameEn = nameEn?.Trim();
            Address = address?.Trim();
            Phone = phone?.Trim();
            AccountId = accountId;
            IsActive = true;
            IsDefault = false;
        }

        // ── Properties ───────────────────────────────────────────

        /// <summary>Unique warehouse code.</summary>
        public string Code { get; private set; }

        /// <summary>Arabic warehouse name.</summary>
        public string NameAr { get; private set; }

        /// <summary>English warehouse name.</summary>
        public string NameEn { get; private set; }

        /// <summary>Physical address.</summary>
        public string Address { get; private set; }

        /// <summary>Contact phone.</summary>
        public string Phone { get; private set; }

        /// <summary>GL account FK for inventory valuation (optional).</summary>
        public int? AccountId { get; private set; }

        /// <summary>Whether this warehouse is active.</summary>
        public bool IsActive { get; private set; }

        /// <summary>Whether this is the default warehouse for new transactions.</summary>
        public bool IsDefault { get; private set; }

        // ── Domain Methods ───────────────────────────────────────

        /// <summary>Updates warehouse information.</summary>
        public void Update(string nameAr, string nameEn, string address, string phone, int? accountId)
        {
            if (string.IsNullOrWhiteSpace(nameAr))
                throw new InventoryDomainException("اسم المخزن بالعربي مطلوب.");

            NameAr = nameAr.Trim();
            NameEn = nameEn?.Trim();
            Address = address?.Trim();
            Phone = phone?.Trim();
            AccountId = accountId;
        }

        /// <summary>Sets this warehouse as the default.</summary>
        public void SetAsDefault() => IsDefault = true;

        /// <summary>Removes default status.</summary>
        public void ClearDefault() => IsDefault = false;

        /// <summary>تعطيل المخزن — Deactivates the warehouse (cannot deactivate default).</summary>
        public void Deactivate()
        {
            if (IsDefault)
                throw new InventoryDomainException("لا يمكن تعطيل المخزن الافتراضي.");
            IsActive = false;
        }

        /// <summary>تفعيل المخزن — Activates the warehouse.</summary>
        public void Activate() => IsActive = true;
    }
}

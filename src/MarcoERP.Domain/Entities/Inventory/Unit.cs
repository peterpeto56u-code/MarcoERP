using System;
using MarcoERP.Domain.Entities.Common;
using MarcoERP.Domain.Exceptions.Inventory;

namespace MarcoERP.Domain.Entities.Inventory
{
    /// <summary>
    /// Unit of measure (e.g., Piece, Carton, Box, Pack, Kg, Meter).
    /// Each product links to its available units via ProductUnit.
    /// </summary>
    public sealed class Unit : AuditableEntity
    {
        // ── Constructors ─────────────────────────────────────────

        /// <summary>EF Core only.</summary>
        private Unit() { }

        /// <summary>
        /// Creates a new Unit with full invariant validation.
        /// </summary>
        public Unit(string nameAr, string nameEn, string abbreviationAr, string abbreviationEn)
        {
            if (string.IsNullOrWhiteSpace(nameAr))
                throw new InventoryDomainException("اسم الوحدة بالعربي مطلوب.");

            if (string.IsNullOrWhiteSpace(abbreviationAr))
                throw new InventoryDomainException("اختصار الوحدة بالعربي مطلوب.");

            NameAr = nameAr.Trim();
            NameEn = nameEn?.Trim();
            AbbreviationAr = abbreviationAr.Trim();
            AbbreviationEn = abbreviationEn?.Trim();
            IsActive = true;
        }

        // ── Properties ───────────────────────────────────────────

        /// <summary>Arabic unit name (e.g., قطعة, كرتونة).</summary>
        public string NameAr { get; private set; }

        /// <summary>English unit name (e.g., Piece, Carton).</summary>
        public string NameEn { get; private set; }

        /// <summary>Arabic abbreviation (e.g., قط., كرت.).</summary>
        public string AbbreviationAr { get; private set; }

        /// <summary>English abbreviation (e.g., PC, CTN).</summary>
        public string AbbreviationEn { get; private set; }

        /// <summary>Whether this unit is active.</summary>
        public bool IsActive { get; private set; }

        // ── Domain Methods ───────────────────────────────────────

        /// <summary>Updates unit information.</summary>
        public void Update(string nameAr, string nameEn, string abbreviationAr, string abbreviationEn)
        {
            if (string.IsNullOrWhiteSpace(nameAr))
                throw new InventoryDomainException("اسم الوحدة بالعربي مطلوب.");

            if (string.IsNullOrWhiteSpace(abbreviationAr))
                throw new InventoryDomainException("اختصار الوحدة بالعربي مطلوب.");

            NameAr = nameAr.Trim();
            NameEn = nameEn?.Trim();
            AbbreviationAr = abbreviationAr.Trim();
            AbbreviationEn = abbreviationEn?.Trim();
        }

        /// <summary>تعطيل الوحدة — Deactivates this unit of measure.</summary>
        public void Deactivate() => IsActive = false;

        /// <summary>تفعيل الوحدة — Activates this unit of measure.</summary>
        public void Activate() => IsActive = true;
    }
}

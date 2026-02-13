using System;
using System.Collections.Generic;
using MarcoERP.Domain.Entities.Common;
using MarcoERP.Domain.Exceptions.Inventory;

namespace MarcoERP.Domain.Entities.Inventory
{
    /// <summary>
    /// Product category — hierarchical classification of products.
    /// Supports up to 3 levels (e.g., Beverages → Soft Drinks → Carbonated).
    /// </summary>
    public sealed class Category : AuditableEntity
    {
        // ── Private collections ──────────────────────────────────
        private readonly List<Category> _children = new();

        // ── Constructors ─────────────────────────────────────────

        /// <summary>EF Core only.</summary>
        private Category() { }

        /// <summary>
        /// Creates a new Category with full invariant validation.
        /// </summary>
        public Category(
            string nameAr,
            string nameEn,
            int? parentCategoryId,
            int level,
            string description = null)
        {
            if (string.IsNullOrWhiteSpace(nameAr))
                throw new InventoryDomainException("اسم التصنيف بالعربي مطلوب.");

            if (level < 1 || level > 3)
                throw new InventoryDomainException("مستوى التصنيف يجب أن يكون بين 1 و 3.");

            if (level == 1 && parentCategoryId.HasValue)
                throw new InventoryDomainException("تصنيفات المستوى الأول لا تقبل تصنيف أب.");

            if (level > 1 && !parentCategoryId.HasValue)
                throw new InventoryDomainException("التصنيفات الفرعية يجب أن يكون لها تصنيف أب.");

            NameAr = nameAr.Trim();
            NameEn = nameEn?.Trim();
            ParentCategoryId = parentCategoryId;
            Level = level;
            IsActive = true;
            Description = description?.Trim();
            ParentCategory = null;
        }

        // ── Properties ───────────────────────────────────────────

        /// <summary>Arabic category name (required).</summary>
        public string NameAr { get; private set; }

        /// <summary>English category name (optional).</summary>
        public string NameEn { get; private set; }

        /// <summary>Parent category FK (null for root).</summary>
        public int? ParentCategoryId { get; private set; }

        /// <summary>Hierarchy level (1–3).</summary>
        public int Level { get; private set; }

        /// <summary>Whether this category is active.</summary>
        public bool IsActive { get; private set; }

        /// <summary>Optional description.</summary>
        public string Description { get; private set; }

        // ── Navigation Properties ────────────────────────────────

        /// <summary>Parent category.</summary>
        public Category ParentCategory { get; private set; }

        /// <summary>Child categories.</summary>
        public IReadOnlyCollection<Category> Children => _children.AsReadOnly();

        // ── Domain Methods ───────────────────────────────────────

        /// <summary>Updates category information.</summary>
        public void Update(string nameAr, string nameEn, string description)
        {
            if (string.IsNullOrWhiteSpace(nameAr))
                throw new InventoryDomainException("اسم التصنيف بالعربي مطلوب.");

            NameAr = nameAr.Trim();
            NameEn = nameEn?.Trim();
            Description = description?.Trim();
        }

        /// <summary>Deactivates the category.</summary>
        public void Deactivate()
        {
            IsActive = false;
        }

        /// <summary>Activates the category.</summary>
        public void Activate()
        {
            IsActive = true;
        }
    }
}

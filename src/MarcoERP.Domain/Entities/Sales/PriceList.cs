using System;
using System.Collections.Generic;
using System.Linq;
using MarcoERP.Domain.Entities.Common;
using MarcoERP.Domain.Exceptions.Sales;

namespace MarcoERP.Domain.Entities.Sales
{
    /// <summary>
    /// Represents a price list (قائمة أسعار) that groups tiered pricing rules.
    /// Each price list can have multiple tiers based on minimum quantity thresholds.
    /// Lifecycle: Active / Inactive with optional date validity.
    /// </summary>
    public sealed class PriceList : SoftDeletableEntity
    {
        private readonly List<PriceTier> _tiers = new();

        // ── Constructors ────────────────────────────────────────

        /// <summary>EF Core only.</summary>
        private PriceList() { }

        /// <summary>
        /// Creates a new PriceList with full invariant validation.
        /// </summary>
        public PriceList(PriceListDraft draft)
        {
            if (draft == null)
                throw new ArgumentNullException(nameof(draft));

            if (string.IsNullOrWhiteSpace(draft.Code))
                throw new SalesInvoiceDomainException("كود قائمة الأسعار مطلوب.");

            if (string.IsNullOrWhiteSpace(draft.NameAr))
                throw new SalesInvoiceDomainException("اسم قائمة الأسعار بالعربي مطلوب.");

            if (draft.ValidFrom.HasValue && draft.ValidTo.HasValue && draft.ValidTo < draft.ValidFrom)
                throw new SalesInvoiceDomainException("تاريخ الانتهاء يجب أن يكون بعد تاريخ البدء.");

            Code = draft.Code.Trim();
            NameAr = draft.NameAr.Trim();
            NameEn = draft.NameEn?.Trim();
            Description = draft.Description?.Trim();
            ValidFrom = draft.ValidFrom;
            ValidTo = draft.ValidTo;
            IsActive = true;
        }

        // ── Properties ──────────────────────────────────────────

        /// <summary>Unique price list code.</summary>
        public string Code { get; private set; }

        /// <summary>Arabic name (required).</summary>
        public string NameAr { get; private set; }

        /// <summary>English name (optional).</summary>
        public string NameEn { get; private set; }

        /// <summary>Optional description.</summary>
        public string Description { get; private set; }

        /// <summary>Start date of validity (null = always valid from creation).</summary>
        public DateTime? ValidFrom { get; private set; }

        /// <summary>End date of validity (null = no expiry).</summary>
        public DateTime? ValidTo { get; private set; }

        /// <summary>Active flag.</summary>
        public bool IsActive { get; private set; }

        /// <summary>Price tiers in this list.</summary>
        public IReadOnlyCollection<PriceTier> Tiers => _tiers.AsReadOnly();

        // ── Domain Methods ──────────────────────────────────────

        /// <summary>Updates price list information.</summary>
        public void Update(PriceListUpdate update)
        {
            if (update == null)
                throw new ArgumentNullException(nameof(update));

            if (string.IsNullOrWhiteSpace(update.NameAr))
                throw new SalesInvoiceDomainException("اسم قائمة الأسعار بالعربي مطلوب.");

            if (update.ValidFrom.HasValue && update.ValidTo.HasValue && update.ValidTo < update.ValidFrom)
                throw new SalesInvoiceDomainException("تاريخ الانتهاء يجب أن يكون بعد تاريخ البدء.");

            NameAr = update.NameAr.Trim();
            NameEn = update.NameEn?.Trim();
            Description = update.Description?.Trim();
            ValidFrom = update.ValidFrom;
            ValidTo = update.ValidTo;
        }

        /// <summary>Adds a price tier for a product in this list.</summary>
        public PriceTier AddTier(int productId, decimal minimumQuantity, decimal price)
        {
            if (productId <= 0)
                throw new SalesInvoiceDomainException("الصنف مطلوب.");

            if (minimumQuantity < 0)
                throw new SalesInvoiceDomainException("الحد الأدنى للكمية لا يمكن أن يكون سالباً.");

            if (price < 0)
                throw new SalesInvoiceDomainException("السعر لا يمكن أن يكون سالباً.");

            // Check for duplicate tier (same product + same min quantity)
            if (_tiers.Any(t => t.ProductId == productId && t.MinimumQuantity == minimumQuantity))
                throw new SalesInvoiceDomainException(
                    $"يوجد سعر مسجل بالفعل لهذا الصنف عند الكمية {minimumQuantity}.");

            var tier = new PriceTier(productId, minimumQuantity, price);
            _tiers.Add(tier);
            return tier;
        }

        /// <summary>Removes a price tier.</summary>
        public void RemoveTier(PriceTier tier)
        {
            if (!_tiers.Remove(tier))
                throw new SalesInvoiceDomainException("السعر غير موجود في هذه القائمة.");
        }

        /// <summary>
        /// Gets the best matching price for a product given a quantity.
        /// Returns the tier with the highest MinimumQuantity that is ≤ the requested quantity.
        /// Returns null if no matching tier exists.
        /// </summary>
        public PriceTier GetBestPrice(int productId, decimal quantity)
        {
            return _tiers
                .Where(t => t.ProductId == productId && t.MinimumQuantity <= quantity)
                .OrderByDescending(t => t.MinimumQuantity)
                .FirstOrDefault();
        }

        /// <summary>Checks if this price list is currently valid based on dates.</summary>
        public bool IsValidOn(DateTime date)
        {
            if (!IsActive) return false;
            if (ValidFrom.HasValue && date < ValidFrom.Value) return false;
            if (ValidTo.HasValue && date > ValidTo.Value) return false;
            return true;
        }

        /// <summary>Deactivates the price list.</summary>
        public void Deactivate() => IsActive = false;

        /// <summary>Activates the price list.</summary>
        public void Activate() => IsActive = true;

        /// <summary>Soft delete override.</summary>
        public override void SoftDelete(string deletedBy, DateTime deletedAt)
        {
            base.SoftDelete(deletedBy, deletedAt);
        }

        // ── Inner Draft / Update Classes ────────────────────────

        public sealed class PriceListDraft
        {
            public string Code { get; init; }
            public string NameAr { get; init; }
            public string NameEn { get; init; }
            public string Description { get; init; }
            public DateTime? ValidFrom { get; init; }
            public DateTime? ValidTo { get; init; }
        }

        public sealed class PriceListUpdate
        {
            public string NameAr { get; init; }
            public string NameEn { get; init; }
            public string Description { get; init; }
            public DateTime? ValidFrom { get; init; }
            public DateTime? ValidTo { get; init; }
        }
    }
}

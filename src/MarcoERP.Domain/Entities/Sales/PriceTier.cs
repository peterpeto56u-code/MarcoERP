using System;
using MarcoERP.Domain.Entities.Common;
using MarcoERP.Domain.Exceptions.Sales;

namespace MarcoERP.Domain.Entities.Sales
{
    /// <summary>
    /// Represents a single price tier within a PriceList.
    /// A tier maps a product + minimum quantity to a specific unit price.
    /// Example: Product X, Qty ≥ 10 → Price = 45.00
    ///          Product X, Qty ≥ 50 → Price = 40.00
    /// </summary>
    public sealed class PriceTier : BaseEntity
    {
        // ── Constructors ────────────────────────────────────────

        /// <summary>EF Core only.</summary>
        private PriceTier() { }

        /// <summary>
        /// Creates a new PriceTier with validation.
        /// </summary>
        internal PriceTier(int productId, decimal minimumQuantity, decimal price)
        {
            if (productId <= 0)
                throw new SalesInvoiceDomainException("الصنف مطلوب.");

            if (minimumQuantity < 0)
                throw new SalesInvoiceDomainException("الحد الأدنى للكمية لا يمكن أن يكون سالباً.");

            if (price < 0)
                throw new SalesInvoiceDomainException("السعر لا يمكن أن يكون سالباً.");

            ProductId = productId;
            MinimumQuantity = minimumQuantity;
            Price = price;
        }

        // ── Properties ──────────────────────────────────────────

        /// <summary>FK to parent PriceList.</summary>
        public int PriceListId { get; private set; }

        /// <summary>FK to Product.</summary>
        public int ProductId { get; private set; }

        /// <summary>
        /// Minimum quantity (in base unit) required to activate this tier.
        /// Zero = default price for any quantity.
        /// </summary>
        public decimal MinimumQuantity { get; private set; }

        /// <summary>Unit price for this tier.</summary>
        public decimal Price { get; private set; }

        // ── Navigation Properties ───────────────────────────────

        /// <summary>Parent price list.</summary>
        public PriceList PriceList { get; private set; }

        /// <summary>Related product.</summary>
        public Inventory.Product Product { get; private set; }

        // ── Domain Methods ──────────────────────────────────────

        /// <summary>Updates the price for this tier.</summary>
        public void UpdatePrice(decimal newPrice)
        {
            if (newPrice < 0)
                throw new SalesInvoiceDomainException("السعر لا يمكن أن يكون سالباً.");

            Price = newPrice;
        }
    }
}

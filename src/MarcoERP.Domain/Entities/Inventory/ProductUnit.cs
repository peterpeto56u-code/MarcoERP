using System;
using MarcoERP.Domain.Entities.Common;
using MarcoERP.Domain.Exceptions.Inventory;

namespace MarcoERP.Domain.Entities.Inventory
{
    /// <summary>
    /// Links a Product to one of its available units of measure.
    /// Each product has a base unit (ConversionFactor = 1) and optional secondary units.
    /// 
    /// Example: Product "Pepsi"
    ///   • Base unit: Piece → ConversionFactor = 1
    ///   • Secondary: Carton → ConversionFactor = 24 (1 carton = 24 pieces)
    ///   • Secondary: Pack → ConversionFactor = 6 (1 pack = 6 pieces)
    /// 
    /// Stock is always stored in base units. Invoices can use any linked unit.
    /// </summary>
    public sealed class ProductUnit : BaseEntity
    {
        // ── Constructors ─────────────────────────────────────────

        /// <summary>EF Core only.</summary>
        private ProductUnit() { }

        /// <summary>
        /// Creates a new Product-Unit relationship with conversion factor.
        /// </summary>
        public ProductUnit(
            int productId,
            int unitId,
            decimal conversionFactor,
            decimal salePrice,
            decimal purchasePrice,
            string barcode = null,
            bool isDefault = false)
        {
            // ProductId may be 0 when adding unit to a new (unsaved) Product — EF sets it via navigation
            if (unitId <= 0)
                throw new InventoryDomainException("الوحدة مطلوبة.");

            if (conversionFactor <= 0)
                throw new InventoryDomainException("معامل التحويل يجب أن يكون أكبر من صفر.");

            if (salePrice < 0)
                throw new InventoryDomainException("سعر البيع لا يمكن أن يكون سالباً.");

            if (purchasePrice < 0)
                throw new InventoryDomainException("سعر الشراء لا يمكن أن يكون سالباً.");

            ProductId = productId;
            UnitId = unitId;
            ConversionFactor = conversionFactor;
            SalePrice = salePrice;
            PurchasePrice = purchasePrice;
            Barcode = barcode?.Trim();
            IsDefault = isDefault;
            Product = null;
            Unit = null;
        }

        // ── Properties ───────────────────────────────────────────

        /// <summary>Product FK.</summary>
        public int ProductId { get; private set; }

        /// <summary>Unit FK.</summary>
        public int UnitId { get; private set; }

        /// <summary>
        /// How many base units equal 1 of this unit.
        /// For the base unit itself, this should be 1.
        /// Example: Carton = 24 → one carton equals 24 pieces.
        /// </summary>
        public decimal ConversionFactor { get; private set; }

        /// <summary>Sale price for 1 of this unit.</summary>
        public decimal SalePrice { get; private set; }

        /// <summary>Purchase price for 1 of this unit.</summary>
        public decimal PurchasePrice { get; private set; }

        /// <summary>Barcode specific to this product-unit combination.</summary>
        public string Barcode { get; private set; }

        /// <summary>Whether this is the default unit shown on invoices.</summary>
        public bool IsDefault { get; private set; }

        // ── Navigation Properties ────────────────────────────────

        /// <summary>Related product.</summary>
        public Product Product { get; private set; }

        /// <summary>Related unit.</summary>
        public Unit Unit { get; private set; }

        // ── Domain Methods ───────────────────────────────────────

        /// <summary>Updates pricing and barcode for this product-unit.</summary>
        public void UpdatePricing(decimal salePrice, decimal purchasePrice, string barcode)
        {
            if (salePrice < 0)
                throw new InventoryDomainException("سعر البيع لا يمكن أن يكون سالباً.");

            if (purchasePrice < 0)
                throw new InventoryDomainException("سعر الشراء لا يمكن أن يكون سالباً.");

            SalePrice = salePrice;
            PurchasePrice = purchasePrice;
            Barcode = barcode?.Trim();
        }

        /// <summary>Updates the conversion factor. Use with extreme caution.</summary>
        public void UpdateConversionFactor(decimal newFactor)
        {
            if (newFactor <= 0)
                throw new InventoryDomainException("معامل التحويل يجب أن يكون أكبر من صفر.");

            ConversionFactor = newFactor;
        }

        /// <summary>Converts a quantity in this unit to base units.</summary>
        public decimal ToBaseUnits(decimal quantity) => quantity * ConversionFactor;

        /// <summary>Converts a quantity in base units to this unit.</summary>
        public decimal FromBaseUnits(decimal baseQuantity) => baseQuantity / ConversionFactor;
    }
}

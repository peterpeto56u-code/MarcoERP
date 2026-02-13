using System;
using FluentAssertions;
using MarcoERP.Domain.Entities.Inventory;
using MarcoERP.Domain.Enums;
using Xunit;

namespace MarcoERP.Domain.Tests.Inventory
{
    public class ProductUnitTests
    {
        [Fact]
        public void Constructor_ValidParameters_CreatesUnit()
        {
            var pu = new ProductUnit(1, 2, 12m, 180m, 120m);
            pu.UnitId.Should().Be(2);
            pu.ConversionFactor.Should().Be(12m);
            pu.SalePrice.Should().Be(180m);
            pu.PurchasePrice.Should().Be(120m);
            pu.IsDefault.Should().BeFalse();
        }

        [Fact]
        public void Constructor_ZeroConversionFactor_ThrowsException()
        {
            Action act = () => new ProductUnit(1, 2, 0m, 180m, 120m);
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void Constructor_NegativeSalePrice_ThrowsException()
        {
            Action act = () => new ProductUnit(1, 2, 12m, -10m, 120m);
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void Constructor_NegativePurchasePrice_ThrowsException()
        {
            Action act = () => new ProductUnit(1, 2, 12m, 180m, -10m);
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void UpdatePricing_ValidPrices_Updates()
        {
            var pu = new ProductUnit(1, 2, 12m, 180m, 120m);
            pu.UpdatePricing(200m, 150m, "999888");
            pu.SalePrice.Should().Be(200m);
            pu.PurchasePrice.Should().Be(150m);
            pu.Barcode.Should().Be("999888");
        }

        [Fact]
        public void UpdatePricing_NegativeSalePrice_ThrowsException()
        {
            var pu = new ProductUnit(1, 2, 12m, 180m, 120m);
            Action act = () => pu.UpdatePricing(-5m, 150m, null);
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void UpdateConversionFactor_ValidFactor_Updates()
        {
            var pu = new ProductUnit(1, 2, 12m, 180m, 120m);
            pu.UpdateConversionFactor(24m);
            pu.ConversionFactor.Should().Be(24m);
        }

        [Fact]
        public void UpdateConversionFactor_ZeroFactor_ThrowsException()
        {
            var pu = new ProductUnit(1, 2, 12m, 180m, 120m);
            Action act = () => pu.UpdateConversionFactor(0m);
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void ToBaseUnits_CalculatesCorrectly()
        {
            var pu = new ProductUnit(1, 2, 12m, 180m, 120m);
            pu.ToBaseUnits(5m).Should().Be(60m); // 5 * 12 = 60
        }

        [Fact]
        public void FromBaseUnits_CalculatesCorrectly()
        {
            var pu = new ProductUnit(1, 2, 12m, 180m, 120m);
            pu.FromBaseUnits(60m).Should().Be(5m); // 60 / 12 = 5
        }
    }

    public class InventoryMovementTests
    {
        private InventoryMovement CreateValidMovement(MovementType type = MovementType.PurchaseIn)
        {
            return new InventoryMovement(
                productId: 1, warehouseId: 1, unitId: 1,
                movementType: type,
                quantity: 10m, quantityInBaseUnit: 10m,
                unitCost: 100m, totalCost: 1000m,
                movementDate: new DateTime(2026, 2, 1),
                referenceNumber: "PI-001",
                sourceType: SourceType.PurchaseInvoice,
                sourceId: 1);
        }

        [Fact]
        public void Constructor_ValidParameters_CreatesMovement()
        {
            var m = CreateValidMovement();
            m.ProductId.Should().Be(1);
            m.Quantity.Should().Be(10m);
            m.TotalCost.Should().Be(1000m);
            m.BalanceAfter.Should().Be(0m);
        }

        [Fact]
        public void Constructor_ZeroQuantity_ThrowsException()
        {
            Action act = () => new InventoryMovement(1, 1, 1, MovementType.PurchaseIn,
                0m, 0m, 100m, 0m, DateTime.Now, "REF", SourceType.PurchaseInvoice);
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void Constructor_EmptyReferenceNumber_ThrowsException()
        {
            Action act = () => new InventoryMovement(1, 1, 1, MovementType.PurchaseIn,
                10m, 10m, 100m, 1000m, DateTime.Now, "", SourceType.PurchaseInvoice);
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void SetBalanceAfter_ValidBalance_Sets()
        {
            var m = CreateValidMovement();
            m.SetBalanceAfter(50m);
            m.BalanceAfter.Should().Be(50m);
        }

        [Fact]
        public void SetBalanceAfter_NegativeBalance_ThrowsException()
        {
            var m = CreateValidMovement();
            Action act = () => m.SetBalanceAfter(-1m);
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void IsIncoming_PurchaseIn_ReturnsTrue()
        {
            var m = CreateValidMovement(MovementType.PurchaseIn);
            m.IsIncoming().Should().BeTrue();
        }

        [Fact]
        public void IsIncoming_SalesReturn_ReturnsTrue()
        {
            var m = CreateValidMovement(MovementType.SalesReturn);
            m.IsIncoming().Should().BeTrue();
        }

        [Fact]
        public void IsOutgoing_SaleOut_ReturnsTrue()
        {
            var m = CreateValidMovement(MovementType.SalesOut);
            m.IsOutgoing().Should().BeTrue();
        }

        [Fact]
        public void IsOutgoing_PurchaseIn_ReturnsFalse()
        {
            var m = CreateValidMovement(MovementType.PurchaseIn);
            m.IsOutgoing().Should().BeFalse();
        }
    }
}

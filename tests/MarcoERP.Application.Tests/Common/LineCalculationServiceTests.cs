using System.Collections.Generic;
using FluentAssertions;
using MarcoERP.Application.DTOs.Common;
using MarcoERP.Application.Services.Common;
using MarcoERP.Domain.Entities.Sales;
using Xunit;

namespace MarcoERP.Application.Tests.Common
{
    public sealed class LineCalculationServiceTests
    {
        [Fact]
        public void CalculateLine_MatchesDomainSalesInvoiceLine()
        {
            var request = new LineCalculationRequest
            {
                Quantity = 2.5m,
                UnitPrice = 10.1234m,
                DiscountPercent = 5m,
                VatRate = 14m,
                ConversionFactor = 3m
            };

            var expected = new SalesInvoiceLine(
                productId: 1,
                unitId: 1,
                quantity: request.Quantity,
                unitPrice: request.UnitPrice,
                conversionFactor: request.ConversionFactor,
                discountPercent: request.DiscountPercent,
                vatRate: request.VatRate);

            var sut = new LineCalculationService();
            var result = sut.CalculateLine(request);

            result.BaseQuantity.Should().Be(expected.BaseQuantity);
            result.SubTotal.Should().Be(expected.SubTotal);
            result.DiscountAmount.Should().Be(expected.DiscountAmount);
            result.NetTotal.Should().Be(expected.NetTotal);
            result.VatAmount.Should().Be(expected.VatAmount);
            result.TotalWithVat.Should().Be(expected.TotalWithVat);
        }

        [Fact]
        public void CalculateTotals_SumsLineResults()
        {
            var lines = new List<LineCalculationRequest>
            {
                new LineCalculationRequest
                {
                    Quantity = 1m,
                    UnitPrice = 100m,
                    DiscountPercent = 10m,
                    VatRate = 14m,
                    ConversionFactor = 1m
                },
                new LineCalculationRequest
                {
                    Quantity = 2m,
                    UnitPrice = 50m,
                    DiscountPercent = 0m,
                    VatRate = 14m,
                    ConversionFactor = 1m
                }
            };

            var sut = new LineCalculationService();
            var totals = sut.CalculateTotals(lines);

            var line1 = sut.CalculateLine(lines[0]);
            var line2 = sut.CalculateLine(lines[1]);

            totals.Subtotal.Should().Be(line1.SubTotal + line2.SubTotal);
            totals.DiscountTotal.Should().Be(line1.DiscountAmount + line2.DiscountAmount);
            totals.VatTotal.Should().Be(line1.VatAmount + line2.VatAmount);
            totals.NetTotal.Should().Be(line1.TotalWithVat + line2.TotalWithVat);
        }

        [Fact]
        public void CalculateLine_WhenConversionFactorZero_DefaultsToOne()
        {
            var request = new LineCalculationRequest
            {
                Quantity = 3m,
                UnitPrice = 10m,
                DiscountPercent = 0m,
                VatRate = 10m,
                ConversionFactor = 0m
            };

            var sut = new LineCalculationService();
            var result = sut.CalculateLine(request);

            result.BaseQuantity.Should().Be(3m);
            result.SubTotal.Should().Be(30m);
            result.VatAmount.Should().Be(3m);
            result.TotalWithVat.Should().Be(33m);
        }

        [Fact]
        public void CalculateLine_WithFullDiscount_ZeroesVatAndNet()
        {
            var request = new LineCalculationRequest
            {
                Quantity = 2m,
                UnitPrice = 50m,
                DiscountPercent = 100m,
                VatRate = 14m,
                ConversionFactor = 1m
            };

            var sut = new LineCalculationService();
            var result = sut.CalculateLine(request);

            result.DiscountAmount.Should().Be(100m);
            result.NetTotal.Should().Be(0m);
            result.VatAmount.Should().Be(0m);
            result.TotalWithVat.Should().Be(0m);
        }

        // ═══════════════════════════════════════════════════════════
        //  PHASE 9F: Profit Calculation & Unit Conversion Tests
        // ═══════════════════════════════════════════════════════════

        [Fact]
        public void CalculateLine_ProfitFields_NoDiscount()
        {
            var sut = new LineCalculationService();
            var result = sut.CalculateLine(new LineCalculationRequest
            {
                Quantity = 10,
                UnitPrice = 50m,
                DiscountPercent = 0,
                VatRate = 15m,
                ConversionFactor = 1m,
                CostPrice = 30m
            });

            result.CostPerUnit.Should().Be(30m);        // 30 × 1
            result.CostTotal.Should().Be(300m);          // 30 × 10
            result.NetUnitPrice.Should().Be(50m);        // 50 × (1 - 0/100)
            result.UnitProfit.Should().Be(20m);           // 50 - 30
            result.TotalProfit.Should().Be(200m);         // 20 × 10
            result.ProfitMarginPercent.Should().Be(40m);  // 200/500 × 100
        }

        [Fact]
        public void CalculateLine_ProfitFields_WithDiscount()
        {
            var sut = new LineCalculationService();
            var result = sut.CalculateLine(new LineCalculationRequest
            {
                Quantity = 4,
                UnitPrice = 100m,
                DiscountPercent = 20m,
                VatRate = 0,
                ConversionFactor = 1m,
                CostPrice = 60m
            });

            result.NetUnitPrice.Should().Be(80m);        // 100 × 0.8
            result.UnitProfit.Should().Be(20m);           // 80 - 60
            result.TotalProfit.Should().Be(80m);          // 20 × 4
            result.NetTotal.Should().Be(320m);            // 400 - 80
            result.ProfitMarginPercent.Should().Be(25m);  // 80/320 × 100
        }

        [Fact]
        public void CalculateLine_ProfitFields_WithConversionFactor()
        {
            // Carton = 12 pieces, WAC per base unit = 5
            var sut = new LineCalculationService();
            var result = sut.CalculateLine(new LineCalculationRequest
            {
                Quantity = 2,
                UnitPrice = 100m,    // per carton
                DiscountPercent = 0,
                VatRate = 0,
                ConversionFactor = 12m,
                CostPrice = 5m       // per piece (base)
            });

            result.BaseQuantity.Should().Be(24m);         // 2 × 12
            result.CostPerUnit.Should().Be(60m);           // 5 × 12
            result.CostTotal.Should().Be(120m);            // 60 × 2
            result.NetUnitPrice.Should().Be(100m);
            result.UnitProfit.Should().Be(40m);            // 100 - 60
            result.TotalProfit.Should().Be(80m);           // 40 × 2
        }

        [Fact]
        public void CalculateLine_FullDiscount_ZeroProfitMargin()
        {
            var sut = new LineCalculationService();
            var result = sut.CalculateLine(new LineCalculationRequest
            {
                Quantity = 5,
                UnitPrice = 200m,
                DiscountPercent = 100m,
                VatRate = 15m,
                ConversionFactor = 1m,
                CostPrice = 100m
            });

            result.NetUnitPrice.Should().Be(0m);
            result.TotalProfit.Should().BeLessOrEqualTo(0m);
            result.ProfitMarginPercent.Should().Be(0m);   // NetTotal is 0
        }

        [Fact]
        public void CalculateLine_ZeroCostPrice_FullProfit()
        {
            var sut = new LineCalculationService();
            var result = sut.CalculateLine(new LineCalculationRequest
            {
                Quantity = 3,
                UnitPrice = 100m,
                DiscountPercent = 0,
                VatRate = 0,
                ConversionFactor = 1m,
                CostPrice = 0
            });

            result.CostPerUnit.Should().Be(0m);
            result.UnitProfit.Should().Be(100m);
            result.TotalProfit.Should().Be(300m);
            result.ProfitMarginPercent.Should().Be(100m);
        }

        [Fact]
        public void CalculateLine_SellingBelowCost_NegativeProfit()
        {
            var sut = new LineCalculationService();
            var result = sut.CalculateLine(new LineCalculationRequest
            {
                Quantity = 1,
                UnitPrice = 40m,
                DiscountPercent = 0,
                VatRate = 0,
                ConversionFactor = 1m,
                CostPrice = 60m
            });

            result.UnitProfit.Should().Be(-20m);
            result.TotalProfit.Should().Be(-20m);
            result.ProfitMarginPercent.Should().BeApproximately(-50m, 0.01m);
        }

        [Fact]
        public void ConvertQuantity_MultipliesByFactor()
        {
            var sut = new LineCalculationService();
            sut.ConvertQuantity(5m, 12m).Should().Be(60m);
            sut.ConvertQuantity(3m, 1m).Should().Be(3m);
            sut.ConvertQuantity(2.5m, 4m).Should().Be(10m);
        }

        [Fact]
        public void ConvertPrice_DividesByFactor()
        {
            var sut = new LineCalculationService();
            sut.ConvertPrice(120m, 12m).Should().Be(10m);
            sut.ConvertPrice(100m, 1m).Should().Be(100m);
        }

        [Fact]
        public void ConvertPrice_ZeroFactor_ReturnsPrice()
        {
            var sut = new LineCalculationService();
            // Zero factor should be safe (return original value or 0)
            var result = sut.ConvertPrice(100m, 0m);
            // Implementation detail: factor <= 0 → factor = 1
            result.Should().Be(100m);
        }

        [Fact]
        public void CalculateLine_RoundingConsistency_Precision4()
        {
            var sut = new LineCalculationService();
            var result = sut.CalculateLine(new LineCalculationRequest
            {
                Quantity = 3,
                UnitPrice = 33.3333m,
                DiscountPercent = 7.5m,
                VatRate = 15m,
                ConversionFactor = 1m,
                CostPrice = 20m
            });

            // All financial fields should be rounded to 4 decimal places
            result.SubTotal.Should().Be(decimal.Round(3 * 33.3333m, 4));
            result.DiscountAmount.Should().Be(decimal.Round(result.SubTotal * 7.5m / 100m, 4));
            result.VatAmount.Should().Be(decimal.Round(result.NetTotal * 15m / 100m, 4));
        }
    }
}

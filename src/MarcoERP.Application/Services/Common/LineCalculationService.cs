using System;
using System.Collections.Generic;
using System.Linq;
using MarcoERP.Application.Common;
using MarcoERP.Application.DTOs.Common;
using MarcoERP.Application.Interfaces;
using MarcoERP.Domain.Enums;

namespace MarcoERP.Application.Services.Common
{
    /// <summary>
    /// Domain-consistent calculation service for line and invoice totals.
    /// Mirrors rounding behavior used by invoice line entities.
    /// Phase 9B: Extended with profit, cost, and unit conversion calculations.
    /// All business arithmetic is centralized here — ViewModels must not contain math.
    /// </summary>
    [Module(SystemModule.Common)]
    public sealed class LineCalculationService : ILineCalculationService
    {
        private const int Precision = 4;

        public LineCalculationResult CalculateLine(LineCalculationRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var qty = request.Quantity;
            var unitPrice = request.UnitPrice;
            var discountPercent = request.DiscountPercent;
            var vatRate = request.VatRate;
            var conversionFactor = request.ConversionFactor <= 0 ? 1m : request.ConversionFactor;

            var baseQty = Math.Round(qty * conversionFactor, Precision);
            var subTotal = Math.Round(qty * unitPrice, Precision);
            var discountAmount = Math.Round(subTotal * discountPercent / 100m, Precision);

            decimal netTotal;
            decimal vatAmount;
            decimal totalWithVat;

            if (request.IsVatInclusive && vatRate > 0)
            {
                // VAT-inclusive: UnitPrice already contains VAT.
                // Governance formula: LineVAT = LineTotal × (VATRate / (100 + VATRate))
                var inclusiveTotal = subTotal - discountAmount;
                vatAmount = Math.Round(inclusiveTotal * vatRate / (100m + vatRate), Precision);
                netTotal = inclusiveTotal - vatAmount;
                totalWithVat = inclusiveTotal;
            }
            else
            {
                // VAT-exclusive (default): VAT added on top
                netTotal = subTotal - discountAmount;
                vatAmount = Math.Round(netTotal * vatRate / 100m, Precision);
                totalWithVat = netTotal + vatAmount;
            }

            // Phase 9B: Profit calculations
            var costPerUnit = Math.Round(request.CostPrice * conversionFactor, Precision);
            var costTotal = Math.Round(baseQty * request.CostPrice, Precision);

            var discountFactor = 1m - discountPercent / 100m;
            if (discountFactor < 0m) discountFactor = 0m;
            var netUnitPrice = Math.Round(unitPrice * discountFactor, Precision);

            var unitProfit = Math.Round(netUnitPrice - costPerUnit, Precision);
            var totalProfit = Math.Round(unitProfit * qty, Precision);
            var profitMarginPercent = netTotal != 0
                ? Math.Round(totalProfit / netTotal * 100m, 2)
                : 0m;

            return new LineCalculationResult
            {
                BaseQuantity = baseQty,
                SubTotal = subTotal,
                DiscountAmount = discountAmount,
                NetTotal = netTotal,
                VatAmount = vatAmount,
                TotalWithVat = totalWithVat,
                CostPerUnit = costPerUnit,
                CostTotal = costTotal,
                NetUnitPrice = netUnitPrice,
                UnitProfit = unitProfit,
                TotalProfit = totalProfit,
                ProfitMarginPercent = profitMarginPercent
            };
        }

        public InvoiceTotalsResult CalculateTotals(IEnumerable<LineCalculationRequest> lines)
        {
            var results = (lines ?? Enumerable.Empty<LineCalculationRequest>())
                .Select(CalculateLine)
                .ToList();

            return new InvoiceTotalsResult
            {
                Subtotal = results.Sum(r => r.SubTotal),
                DiscountTotal = results.Sum(r => r.DiscountAmount),
                VatTotal = results.Sum(r => r.VatAmount),
                NetTotal = results.Sum(r => r.TotalWithVat)
            };
        }

        /// <inheritdoc />
        public decimal ConvertQuantity(decimal quantity, decimal factor)
        {
            if (factor <= 0) return quantity;
            return Math.Round(quantity * factor, Precision);
        }

        /// <inheritdoc />
        public decimal ConvertPrice(decimal price, decimal factor)
        {
            if (factor <= 0) return price;
            return Math.Round(price / factor, Precision);
        }
    }
}

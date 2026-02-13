using System.Collections.Generic;
using MarcoERP.Application.DTOs.Common;

namespace MarcoERP.Application.Interfaces
{
    /// <summary>
    /// Provides consistent line and invoice total calculations.
    /// Phase 9B: Extended with unit conversion and profit calculations.
    /// No arithmetic should exist in ViewModels — all math goes through this service.
    /// </summary>
    public interface ILineCalculationService
    {
        /// <summary>
        /// Calculates totals for a single line using domain-consistent rounding.
        /// Includes profit fields when <see cref="LineCalculationRequest.CostPrice"/> is set.
        /// </summary>
        LineCalculationResult CalculateLine(LineCalculationRequest request);

        /// <summary>
        /// Calculates invoice totals from a list of line inputs.
        /// </summary>
        InvoiceTotalsResult CalculateTotals(IEnumerable<LineCalculationRequest> lines);

        /// <summary>
        /// Converts quantity from one unit to another using a conversion factor.
        /// E.g., primary qty → secondary qty: qty × factor.
        /// </summary>
        decimal ConvertQuantity(decimal quantity, decimal factor);

        /// <summary>
        /// Converts price from one unit to another using a conversion factor.
        /// E.g., primary price → secondary price: price / factor.
        /// </summary>
        decimal ConvertPrice(decimal price, decimal factor);
    }
}

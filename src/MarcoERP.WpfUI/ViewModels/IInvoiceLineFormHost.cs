using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using MarcoERP.Application.DTOs.Common;
using MarcoERP.Application.DTOs.Inventory;

namespace MarcoERP.WpfUI.ViewModels
{
    /// <summary>
    /// Interface for ViewModels that host invoice line form items.
    /// Allows line items to access the product list and trigger total recalculation
    /// without being coupled to a specific ViewModel type.
    /// </summary>
    public interface IInvoiceLineFormHost
    {
        /// <summary>Available products for line item selection.</summary>
        ObservableCollection<ProductDto> Products { get; }

        /// <summary>Recalculates header totals when a line changes.</summary>
        void RefreshTotals();

        /// <summary>Calculates totals for a single line.</summary>
        LineCalculationResult CalculateLine(LineCalculationRequest request);

        /// <summary>Calculates totals for all lines.</summary>
        InvoiceTotalsResult CalculateTotals(IEnumerable<LineCalculationRequest> lines);

        /// <summary>Converts a quantity by multiplication (qty × factor). Phase 9.</summary>
        decimal ConvertQuantity(decimal quantity, decimal factor);

        /// <summary>Converts a price by division (price / factor). Phase 9.</summary>
        decimal ConvertPrice(decimal price, decimal factor);

        /// <summary>Refreshes the Products collection from the database.</summary>
        Task RefreshProductsAsync();
    }
}

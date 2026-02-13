using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MarcoERP.Application.Common;
using MarcoERP.Application.DTOs.Purchases;

namespace MarcoERP.Application.Interfaces.Purchases
{
    /// <summary>
    /// Application service contract for Purchase Invoice operations.
    /// Handles CRUD, posting (auto-journal + WAC + stock), and cancellation.
    /// </summary>
    public interface IPurchaseInvoiceService
    {
        /// <summary>Gets all purchase invoices (list view).</summary>
        Task<ServiceResult<IReadOnlyList<PurchaseInvoiceListDto>>> GetAllAsync(CancellationToken ct = default);

        /// <summary>Gets a purchase invoice with all lines.</summary>
        Task<ServiceResult<PurchaseInvoiceDto>> GetByIdAsync(int id, CancellationToken ct = default);

        /// <summary>Gets the next auto-generated invoice number.</summary>
        Task<ServiceResult<string>> GetNextNumberAsync(CancellationToken ct = default);

        /// <summary>Creates a new draft purchase invoice.</summary>
        Task<ServiceResult<PurchaseInvoiceDto>> CreateAsync(CreatePurchaseInvoiceDto dto, CancellationToken ct = default);

        /// <summary>Updates a draft purchase invoice.</summary>
        Task<ServiceResult<PurchaseInvoiceDto>> UpdateAsync(UpdatePurchaseInvoiceDto dto, CancellationToken ct = default);

        /// <summary>
        /// Posts a draft invoice: generates journal entry, updates WAC, creates inventory movements.
        /// </summary>
        Task<ServiceResult<PurchaseInvoiceDto>> PostAsync(int id, CancellationToken ct = default);

        /// <summary>Cancels a posted invoice (reversal journal + stock reversal).</summary>
        Task<ServiceResult> CancelAsync(int id, CancellationToken ct = default);

        /// <summary>Deletes a draft invoice (hard delete — not yet posted).</summary>
        Task<ServiceResult> DeleteDraftAsync(int id, CancellationToken ct = default);
    }
}

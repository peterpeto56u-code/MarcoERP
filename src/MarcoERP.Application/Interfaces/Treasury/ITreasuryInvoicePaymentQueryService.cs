using System.Threading;
using System.Threading.Tasks;
using MarcoERP.Application.Common;

namespace MarcoERP.Application.Interfaces.Treasury
{
    public interface ITreasuryInvoicePaymentQueryService
    {
        Task<ServiceResult<decimal>> GetPostedReceiptsTotalForSalesInvoiceAsync(int salesInvoiceId, CancellationToken ct = default);
        Task<ServiceResult<decimal>> GetPostedPaymentsTotalForPurchaseInvoiceAsync(int purchaseInvoiceId, CancellationToken ct = default);
    }
}

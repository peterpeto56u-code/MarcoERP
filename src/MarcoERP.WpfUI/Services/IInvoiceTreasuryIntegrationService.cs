using System.Threading;
using System.Threading.Tasks;
using MarcoERP.Application.DTOs.Purchases;
using MarcoERP.Application.DTOs.Sales;
using MarcoERP.Domain.Enums;

namespace MarcoERP.WpfUI.Services
{
    public sealed record InvoiceTreasuryCreateResult(bool Created, string ErrorMessage);

    public interface IInvoiceTreasuryIntegrationService
    {
        Task<InvoiceTreasuryCreateResult> PromptAndCreateSalesReceiptAsync(SalesInvoiceDto invoice, int customerAccountId, CancellationToken ct = default);
        Task<InvoiceTreasuryCreateResult> PromptAndCreatePurchasePaymentAsync(PurchaseInvoiceDto invoice, int supplierAccountId, CancellationToken ct = default);

        Task<decimal> GetPostedPaidForSalesInvoiceAsync(int salesInvoiceId, CancellationToken ct = default);
        Task<decimal> GetPostedPaidForPurchaseInvoiceAsync(int purchaseInvoiceId, CancellationToken ct = default);

        string GetPaymentMethodLabel(PaymentMethod method);
    }
}

using System.Threading.Tasks;
using MarcoERP.Application.DTOs.Purchases;
using MarcoERP.Application.DTOs.Sales;

namespace MarcoERP.WpfUI.Services
{
    public interface IInvoicePdfPreviewService
    {
        Task ShowSalesInvoiceAsync(SalesInvoiceDto invoice);
        Task ShowPurchaseInvoiceAsync(PurchaseInvoiceDto invoice);

        /// <summary>Shows a generic HTML preview with PDF generation.</summary>
        Task ShowHtmlPreviewAsync(InvoicePdfPreviewRequest request);
    }
}

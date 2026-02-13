using System;

namespace MarcoERP.WpfUI.Navigation
{
    public sealed class CashReceiptNavigationParams
    {
        public int? SalesInvoiceId { get; init; }
        public int? CustomerId { get; init; }
        public DateTime? Date { get; init; }
        public decimal? Amount { get; init; }
        public string Description { get; init; }
        public string Notes { get; init; }
    }

    public sealed class CashPaymentNavigationParams
    {
        public int? PurchaseInvoiceId { get; init; }
        public int? SupplierId { get; init; }
        public DateTime? Date { get; init; }
        public decimal? Amount { get; init; }
        public string Description { get; init; }
        public string Notes { get; init; }
    }
}

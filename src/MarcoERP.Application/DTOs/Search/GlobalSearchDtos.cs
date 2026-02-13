namespace MarcoERP.Application.DTOs.Search
{
    public enum GlobalSearchEntityType
    {
        Customer = 1,
        Product = 2,
        SalesInvoice = 3,
        PurchaseInvoice = 4,
        JournalEntry = 5,
        CashReceipt = 6,
        CashPayment = 7,
        Supplier = 8,
    }

    public sealed class GlobalSearchHitDto
    {
        public GlobalSearchEntityType EntityType { get; set; }

        public int Id { get; set; }

        public string Title { get; set; }

        public string Subtitle { get; set; }
    }
}

namespace MarcoERP.Domain.Enums
{
    /// <summary>
    /// Lifecycle status for purchase and sales invoices.
    /// Draft → Posted → (optionally) Cancelled.
    /// </summary>
    public enum InvoiceStatus
    {
        /// <summary>Invoice is being prepared; can be edited or deleted.</summary>
        Draft = 0,

        /// <summary>Invoice has been posted; journal entry generated, stock updated. Immutable.</summary>
        Posted = 1,

        /// <summary>Invoice has been cancelled; reversal journal entry generated.</summary>
        Cancelled = 2
    }
}

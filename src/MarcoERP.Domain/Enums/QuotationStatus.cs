namespace MarcoERP.Domain.Enums
{
    /// <summary>
    /// Lifecycle status for sales and purchase quotations.
    /// Draft → Sent → Accepted / Rejected → (Converted) or (Expired / Cancelled).
    /// </summary>
    public enum QuotationStatus
    {
        /// <summary>Quotation is being prepared; can be edited or deleted.</summary>
        Draft = 0,

        /// <summary>Quotation has been sent to the customer/supplier.</summary>
        Sent = 1,

        /// <summary>Quotation has been accepted by the customer/supplier.</summary>
        Accepted = 2,

        /// <summary>Quotation has been rejected by the customer/supplier.</summary>
        Rejected = 3,

        /// <summary>Quotation has been converted to an invoice.</summary>
        Converted = 4,

        /// <summary>Quotation was manually cancelled.</summary>
        Cancelled = 5,

        /// <summary>Quotation validity date has passed without acceptance.</summary>
        Expired = 6
    }
}

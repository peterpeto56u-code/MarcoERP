namespace MarcoERP.Domain.Enums
{
    /// <summary>
    /// Status of a product in the system.
    /// </summary>
    public enum ProductStatus
    {
        /// <summary>Active and available for transactions.</summary>
        Active = 0,

        /// <summary>Temporarily disabled — cannot be used in new invoices.</summary>
        Inactive = 1,

        /// <summary>Discontinued — kept for historical reference only.</summary>
        Discontinued = 2
    }
}

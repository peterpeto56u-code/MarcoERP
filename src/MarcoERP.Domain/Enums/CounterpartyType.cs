namespace MarcoERP.Domain.Enums
{
    /// <summary>
    /// Identifies the type of counterparty on a transaction.
    /// Allows sales invoices to target suppliers and purchase invoices to target customers.
    /// </summary>
    public enum CounterpartyType
    {
        /// <summary>عميل — default for sales invoices.</summary>
        Customer = 0,

        /// <summary>مورد — default for purchase invoices.</summary>
        Supplier = 1,

        /// <summary>مندوب مبيعات — sales representative acting as counterparty.</summary>
        SalesRepresentative = 2
    }
}

namespace MarcoERP.Domain.Enums
{
    /// <summary>
    /// Payment method used in POS transactions.
    /// </summary>
    public enum PaymentMethod
    {
        /// <summary>Cash payment.</summary>
        Cash = 0,

        /// <summary>Card (credit/debit) payment.</summary>
        Card = 1,

        /// <summary>On account — charged to customer AR.</summary>
        OnAccount = 2
    }
}

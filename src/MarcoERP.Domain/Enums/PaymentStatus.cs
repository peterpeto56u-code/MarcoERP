namespace MarcoERP.Domain.Enums
{
    /// <summary>
    /// Payment lifecycle for invoices (فواتير): tracks how much has been paid.
    /// </summary>
    public enum PaymentStatus
    {
        /// <summary>No payment received / made yet.</summary>
        Unpaid = 0,

        /// <summary>Some payment received / made but balance remains.</summary>
        PartiallyPaid = 1,

        /// <summary>Invoice fully settled.</summary>
        FullyPaid = 2
    }
}

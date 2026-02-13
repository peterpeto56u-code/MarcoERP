using System;

namespace MarcoERP.Domain.Exceptions.Purchases
{
    /// <summary>
    /// Domain exception for PurchaseInvoice invariant violations.
    /// </summary>
    public sealed class PurchaseInvoiceDomainException : Exception
    {
        public PurchaseInvoiceDomainException(string message)
            : base(message) { }

        public PurchaseInvoiceDomainException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}

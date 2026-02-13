using System;

namespace MarcoERP.Domain.Exceptions.Purchases
{
    /// <summary>
    /// Domain exception for purchase quotation invariant violations.
    /// </summary>
    public sealed class PurchaseQuotationDomainException : Exception
    {
        public PurchaseQuotationDomainException(string message)
            : base(message) { }

        public PurchaseQuotationDomainException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}

using System;

namespace MarcoERP.Domain.Exceptions.Sales
{
    /// <summary>
    /// Domain exception for sales quotation invariant violations.
    /// </summary>
    public sealed class SalesQuotationDomainException : Exception
    {
        public SalesQuotationDomainException(string message)
            : base(message) { }

        public SalesQuotationDomainException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}

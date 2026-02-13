using System;

namespace MarcoERP.Domain.Exceptions.Sales
{
    /// <summary>
    /// Domain exception for SalesInvoice / SalesReturn invariant violations.
    /// </summary>
    public sealed class SalesInvoiceDomainException : Exception
    {
        public SalesInvoiceDomainException(string message)
            : base(message)
        {
        }

        public SalesInvoiceDomainException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}

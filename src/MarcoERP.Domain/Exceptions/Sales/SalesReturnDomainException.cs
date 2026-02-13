using System;

namespace MarcoERP.Domain.Exceptions.Sales
{
    /// <summary>Domain exception for sales return validation errors.</summary>
    public sealed class SalesReturnDomainException : Exception
    {
        public SalesReturnDomainException(string message) : base(message) { }
        public SalesReturnDomainException(string message, Exception innerException) : base(message, innerException) { }
    }
}

using System;

namespace MarcoERP.Domain.Exceptions.Sales
{
    /// <summary>
    /// Domain exception for SalesRepresentative invariant violations.
    /// </summary>
    public sealed class SalesRepresentativeDomainException : Exception
    {
        public SalesRepresentativeDomainException(string message)
            : base(message) { }

        public SalesRepresentativeDomainException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}

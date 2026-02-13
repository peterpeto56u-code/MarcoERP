using System;

namespace MarcoERP.Domain.Exceptions.Sales
{
    /// <summary>
    /// Domain exception for Customer invariant violations.
    /// </summary>
    public sealed class CustomerDomainException : Exception
    {
        public CustomerDomainException(string message)
            : base(message) { }

        public CustomerDomainException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}

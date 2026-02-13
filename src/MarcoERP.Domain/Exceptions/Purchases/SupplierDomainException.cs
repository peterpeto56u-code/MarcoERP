using System;

namespace MarcoERP.Domain.Exceptions.Purchases
{
    /// <summary>
    /// Domain exception for Supplier invariant violations.
    /// </summary>
    public sealed class SupplierDomainException : Exception
    {
        public SupplierDomainException(string message)
            : base(message) { }

        public SupplierDomainException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}

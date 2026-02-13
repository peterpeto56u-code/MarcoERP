using System;

namespace MarcoERP.Domain.Exceptions.Purchases
{
    /// <summary>Domain exception for purchase return validation errors.</summary>
    public sealed class PurchaseReturnDomainException : Exception
    {
        public PurchaseReturnDomainException(string message) : base(message) { }
        public PurchaseReturnDomainException(string message, Exception innerException) : base(message, innerException) { }
    }
}

using System;

namespace MarcoERP.Domain.Exceptions.Treasury
{
    /// <summary>
    /// Domain exception for treasury-related invariant violations
    /// (Cashbox, CashReceipt, CashPayment, CashTransfer).
    /// </summary>
    public sealed class TreasuryDomainException : Exception
    {
        public TreasuryDomainException(string message) : base(message) { }
        public TreasuryDomainException(string message, Exception innerException) : base(message, innerException) { }
    }
}

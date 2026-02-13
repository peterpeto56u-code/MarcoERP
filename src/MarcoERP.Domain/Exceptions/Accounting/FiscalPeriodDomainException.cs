using System;

namespace MarcoERP.Domain.Exceptions.Accounting
{
    /// <summary>
    /// Domain exception for FiscalPeriod invariant violations.
    /// </summary>
    public sealed class FiscalPeriodDomainException : Exception
    {
        public FiscalPeriodDomainException(string message) : base(message)
        {
        }
    }
}

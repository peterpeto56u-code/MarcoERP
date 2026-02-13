using System;

namespace MarcoERP.Domain.Exceptions.Accounting
{
    /// <summary>
    /// Domain exception for FiscalYear invariant violations.
    /// </summary>
    public sealed class FiscalYearDomainException : Exception
    {
        public FiscalYearDomainException(string message) : base(message)
        {
        }
    }
}

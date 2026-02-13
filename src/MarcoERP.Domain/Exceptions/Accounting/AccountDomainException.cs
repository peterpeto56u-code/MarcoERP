using System;

namespace MarcoERP.Domain.Exceptions.Accounting
{
    public sealed class AccountDomainException : Exception
    {
        public AccountDomainException(string message) : base(message)
        {
        }
    }
}

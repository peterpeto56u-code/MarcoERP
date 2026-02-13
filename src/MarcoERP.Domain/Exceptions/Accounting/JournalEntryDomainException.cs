using System;

namespace MarcoERP.Domain.Exceptions.Accounting
{
    public sealed class JournalEntryDomainException : Exception
    {
        public JournalEntryDomainException(string message) : base(message)
        {
        }
    }
}

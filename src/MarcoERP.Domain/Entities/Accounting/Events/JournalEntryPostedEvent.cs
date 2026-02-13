using System;

namespace MarcoERP.Domain.Entities.Accounting.Events
{
    /// <summary>
    /// Raised when a journal entry transitions to Posted status.
    /// </summary>
    public sealed class JournalEntryPostedEvent : IDomainEvent
    {
        public JournalEntryPostedEvent(int journalEntryId, DateTime occurredOnUtc)
        {
            JournalEntryId = journalEntryId;
            OccurredOnUtc = occurredOnUtc;
        }

        /// <summary>Id of the posted journal entry.</summary>
        public int JournalEntryId { get; }

        /// <summary>UTC timestamp.</summary>
        public DateTime OccurredOnUtc { get; }
    }
}

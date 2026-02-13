using System;

namespace MarcoERP.Domain.Entities.Accounting.Events
{
    /// <summary>
    /// Marker interface for domain events raised by entities.
    /// </summary>
    public interface IDomainEvent
    {
        /// <summary>UTC timestamp when the event occurred.</summary>
        DateTime OccurredOnUtc { get; }
    }
}

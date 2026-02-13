using System;

namespace MarcoERP.Application.Interfaces
{
    /// <summary>
    /// Abstraction for system clock — enables testability.
    /// Implementation: Infrastructure layer.
    /// </summary>
    public interface IDateTimeProvider
    {
        /// <summary>Returns the current UTC date/time.</summary>
        DateTime UtcNow { get; }

        /// <summary>Returns the current local date (for journal dates).</summary>
        DateTime Today { get; }
    }
}

using System;
using MarcoERP.Application.Interfaces;

namespace MarcoERP.Infrastructure.Services
{
    /// <summary>
    /// Production implementation of IDateTimeProvider using the system clock.
    /// Enables testability by abstracting DateTime.UtcNow / DateTime.Today.
    /// </summary>
    public sealed class DateTimeProvider : IDateTimeProvider
    {
        /// <inheritdoc />
        public DateTime UtcNow => DateTime.UtcNow;

        /// <inheritdoc />
        public DateTime Today => DateTime.Today;
    }
}

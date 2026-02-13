using System;
using MarcoERP.Application.Interfaces;

namespace MarcoERP.Infrastructure.Services
{
    /// <summary>
    /// Default IActivityTracker implementation.
    /// In WPF, the UI layer calls <see cref="RecordActivity"/> from input hooks.
    /// </summary>
    public sealed class ActivityTracker : IActivityTracker
    {
        private readonly IDateTimeProvider _dateTime;
        private DateTime _lastActivityUtc;

        public ActivityTracker(IDateTimeProvider dateTime)
        {
            _dateTime = dateTime ?? throw new ArgumentNullException(nameof(dateTime));
            _lastActivityUtc = _dateTime.UtcNow;
        }

        /// <inheritdoc />
        public DateTime LastActivityUtc => _lastActivityUtc;

        /// <inheritdoc />
        public void RecordActivity()
        {
            _lastActivityUtc = _dateTime.UtcNow;
        }

        /// <inheritdoc />
        public bool IsIdle(TimeSpan timeout)
        {
            return (_dateTime.UtcNow - _lastActivityUtc) >= timeout;
        }
    }
}

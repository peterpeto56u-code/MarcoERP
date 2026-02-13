using System;
using System.Collections.Generic;
using System.Linq;
using MarcoERP.Application.Interfaces;

namespace MarcoERP.Infrastructure.Services
{
    /// <summary>
    /// Thread-safe in-memory alert store.
    /// Singleton — shared between background jobs and the UI.
    /// </summary>
    public sealed class AlertService : IAlertService
    {
        private readonly object _lock = new();
        private readonly List<AlertItem> _alerts = new();
        private readonly IDateTimeProvider _dateTime;

        public AlertService(IDateTimeProvider dateTime)
        {
            _dateTime = dateTime ?? throw new ArgumentNullException(nameof(dateTime));
        }

        /// <inheritdoc />
        public IReadOnlyList<AlertItem> ActiveAlerts
        {
            get
            {
                lock (_lock)
                {
                    return _alerts.ToList().AsReadOnly();
                }
            }
        }

        /// <inheritdoc />
        public event EventHandler AlertsChanged;

        /// <inheritdoc />
        public void AddAlert(string message, string category, AlertSeverity severity)
        {
            if (string.IsNullOrWhiteSpace(message)) return;

            lock (_lock)
            {
                _alerts.Add(new AlertItem
                {
                    Message = message,
                    Category = category ?? string.Empty,
                    Severity = severity,
                    Timestamp = _dateTime.UtcNow
                });
            }

            AlertsChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <inheritdoc />
        public void ClearAlerts(string category)
        {
            if (string.IsNullOrWhiteSpace(category)) return;

            bool removed;
            lock (_lock)
            {
                removed = _alerts.RemoveAll(a =>
                    string.Equals(a.Category, category, StringComparison.OrdinalIgnoreCase)) > 0;
            }

            if (removed)
            {
                AlertsChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <inheritdoc />
        public void ClearAll()
        {
            bool hadItems;
            lock (_lock)
            {
                hadItems = _alerts.Count > 0;
                _alerts.Clear();
            }

            if (hadItems)
            {
                AlertsChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}

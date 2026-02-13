using System;
using System.Collections.Generic;

namespace MarcoERP.Application.Interfaces
{
    /// <summary>
    /// In-memory alert hub consumed by background jobs and displayed by the UI.
    /// Implementation: Infrastructure layer (thread-safe list).
    /// </summary>
    public interface IAlertService
    {
        /// <summary>Current list of active alerts.</summary>
        IReadOnlyList<AlertItem> ActiveAlerts { get; }

        /// <summary>Raised whenever alerts are added or cleared.</summary>
        event EventHandler AlertsChanged;

        /// <summary>Adds a new alert.</summary>
        void AddAlert(string message, string category, AlertSeverity severity);

        /// <summary>Removes all alerts for the given category.</summary>
        void ClearAlerts(string category);

        /// <summary>Removes all alerts.</summary>
        void ClearAll();
    }

    /// <summary>Alert severity levels.</summary>
    public enum AlertSeverity
    {
        Info,
        Warning,
        Critical
    }

    /// <summary>Represents a single alert displayed to the user.</summary>
    public class AlertItem
    {
        public string Message { get; set; }
        public string Category { get; set; }
        public AlertSeverity Severity { get; set; }
        public DateTime Timestamp { get; set; }
    }
}

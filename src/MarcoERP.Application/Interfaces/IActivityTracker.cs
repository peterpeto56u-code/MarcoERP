using System;

namespace MarcoERP.Application.Interfaces
{
    /// <summary>
    /// Tracks user activity for idle detection.
    /// Implementation: WPF layer (hooks mouse/keyboard input).
    /// </summary>
    public interface IActivityTracker
    {
        /// <summary>UTC timestamp of the last detected user activity.</summary>
        DateTime LastActivityUtc { get; }

        /// <summary>Records a new activity event (call from input hooks).</summary>
        void RecordActivity();

        /// <summary>Returns true if no activity has been recorded within the given timeout.</summary>
        bool IsIdle(TimeSpan timeout);
    }
}

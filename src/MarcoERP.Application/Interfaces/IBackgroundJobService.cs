using System;

namespace MarcoERP.Application.Interfaces
{
    /// <summary>
    /// Manages periodic background jobs (auto-backup, session watchdog, low-stock alerts).
    /// Implementation: Infrastructure layer using System.Timers.Timer.
    /// </summary>
    public interface IBackgroundJobService : IDisposable
    {
        /// <summary>Starts all registered background jobs.</summary>
        void StartAll();

        /// <summary>Stops all running background jobs.</summary>
        void StopAll();
    }
}

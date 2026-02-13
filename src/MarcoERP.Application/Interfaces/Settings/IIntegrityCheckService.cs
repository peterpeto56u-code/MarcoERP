using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MarcoERP.Application.Interfaces.Settings
{
    /// <summary>
    /// Runs system-level governance integrity checks.
    /// Phase 5: Version &amp; Integrity Engine — read-only validation.
    /// </summary>
    public interface IIntegrityCheckService
    {
        /// <summary>
        /// Executes all checks and returns a list of warnings/results.
        /// Does NOT prevent system operation — informational only.
        /// </summary>
        Task<List<IntegrityCheckResult>> RunChecksAsync(CancellationToken ct = default);
    }

    /// <summary>
    /// A single integrity check result.
    /// </summary>
    public sealed class IntegrityCheckResult
    {
        public string CheckName { get; set; }
        public string Status { get; set; }     // OK, Warning, Critical
        public string Message { get; set; }
    }
}

using System.Threading;
using System.Threading.Tasks;

namespace MarcoERP.Application.Interfaces.Settings
{
    /// <summary>
    /// Phase 7: Writes governance-specific audit log entries.
    /// Operates independently of ICurrentUserService (session isolation).
    /// </summary>
    public interface IGovernanceAuditService
    {
        /// <summary>
        /// Logs a governance action (access granted, access denied, feature toggle, etc.).
        /// </summary>
        /// <param name="action">The action performed (e.g., "GovernanceAccessGranted").</param>
        /// <param name="performedBy">Username of the authenticated governance user.</param>
        /// <param name="details">Additional details about the action.</param>
        /// <param name="ct">Cancellation token.</param>
        Task LogAsync(string action, string performedBy, string details = null, CancellationToken ct = default);
    }
}

using System;
using System.Threading;
using System.Threading.Tasks;
using MarcoERP.Application.Interfaces;
using MarcoERP.Application.Interfaces.Settings;
using MarcoERP.Domain.Entities.Accounting;

namespace MarcoERP.Persistence.Services.Settings
{
    /// <summary>
    /// Phase 7: Writes governance-specific audit log entries directly to the AuditLogs table.
    /// Uses IDateTimeProvider instead of ICurrentUserService (session isolation — 7E).
    /// </summary>
    public sealed class GovernanceAuditService : IGovernanceAuditService
    {
        private readonly MarcoDbContext _db;
        private readonly IDateTimeProvider _dateTimeProvider;

        public GovernanceAuditService(MarcoDbContext db, IDateTimeProvider dateTimeProvider)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
        }

        public async Task LogAsync(string action, string performedBy, string details = null, CancellationToken ct = default)
        {
            var entry = new AuditLog(
                entityType: "GovernanceConsole",
                entityId: 0,
                action: action,
                performedBy: performedBy ?? "Unknown",
                timestamp: _dateTimeProvider.UtcNow,
                details: details);

            _db.AuditLogs.Add(entry);
            await _db.SaveChangesAsync(ct);
        }
    }
}

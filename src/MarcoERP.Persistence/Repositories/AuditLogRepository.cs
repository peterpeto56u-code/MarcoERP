using System.Threading;
using System.Threading.Tasks;
using MarcoERP.Domain.Entities.Accounting;

namespace MarcoERP.Persistence.Repositories
{
    /// <summary>
    /// EF Core implementation of IAuditLogRepository.
    /// Append-only — no update or delete methods.
    /// </summary>
    public sealed class AuditLogRepository : Domain.Interfaces.IAuditLogRepository
    {
        private readonly MarcoDbContext _context;

        public AuditLogRepository(MarcoDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
        {
            await _context.AuditLogs.AddAsync(auditLog, cancellationToken);
        }
    }
}

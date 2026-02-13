using System.Threading;
using System.Threading.Tasks;
using MarcoERP.Application.Interfaces;
using MarcoERP.Domain.Entities.Accounting;
using MarcoERP.Domain.Interfaces;

namespace MarcoERP.Infrastructure.Services
{
    /// <summary>
    /// Implementation of IAuditLogger.
    /// Writes immutable AuditLog records via the generic repository / UnitOfWork.
    /// TRX-INT-05: Audit record is inserted within the same transaction as the business operation.
    /// The caller (Application service) must call UoW.SaveChangesAsync to commit.
    /// </summary>
    public sealed class AuditLogger : IAuditLogger
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IDateTimeProvider _dateTimeProvider;

        public AuditLogger(
            IAuditLogRepository auditLogRepository,
            IUnitOfWork unitOfWork,
            IDateTimeProvider dateTimeProvider)
        {
            _auditLogRepository = auditLogRepository;
            _unitOfWork = unitOfWork;
            _dateTimeProvider = dateTimeProvider;
        }

        /// <inheritdoc />
        public async Task LogAsync(
            string entityType,
            int entityId,
            string action,
            string performedBy,
            string details = null,
            CancellationToken cancellationToken = default)
        {
            var log = new AuditLog(
                entityType,
                entityId,
                action,
                performedBy,
                _dateTimeProvider.UtcNow,
                details);
            await _auditLogRepository.AddAsync(log, cancellationToken);
            // NOTE: SaveChanges is NOT called here — the calling service commits
            // the entire transaction including audit records atomically.
        }
    }
}

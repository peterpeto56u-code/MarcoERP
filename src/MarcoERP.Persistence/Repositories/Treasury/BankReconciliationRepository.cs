using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MarcoERP.Domain.Entities.Treasury;
using MarcoERP.Domain.Interfaces.Treasury;

namespace MarcoERP.Persistence.Repositories.Treasury
{
    /// <summary>
    /// EF Core implementation of IBankReconciliationRepository.
    /// </summary>
    public sealed class BankReconciliationRepository : IBankReconciliationRepository
    {
        private readonly MarcoDbContext _context;

        public BankReconciliationRepository(MarcoDbContext context) => _context = context;

        // ── IRepository<BankReconciliation> ──────────────────────

        public async Task<BankReconciliation> GetByIdAsync(int id, CancellationToken ct = default)
            => await _context.BankReconciliations.FirstOrDefaultAsync(r => r.Id == id, ct);

        public async Task<IReadOnlyList<BankReconciliation>> GetAllAsync(CancellationToken ct = default)
            => await _context.BankReconciliations
                .Include(r => r.BankAccount)
                .OrderByDescending(r => r.ReconciliationDate)
                .ToListAsync(ct);

        public async Task AddAsync(BankReconciliation entity, CancellationToken ct = default)
            => await _context.BankReconciliations.AddAsync(entity, ct);

        public void Update(BankReconciliation entity) => _context.BankReconciliations.Update(entity);
        public void Remove(BankReconciliation entity) => _context.BankReconciliations.Remove(entity);

        // ── IBankReconciliationRepository ────────────────────────

        public async Task<BankReconciliation> GetByIdWithItemsAsync(int id, CancellationToken ct = default)
            => await _context.BankReconciliations
                .Include(r => r.Items)
                .Include(r => r.BankAccount)
                .FirstOrDefaultAsync(r => r.Id == id, ct);

        public async Task<IReadOnlyList<BankReconciliation>> GetByBankAccountAsync(int bankAccountId, CancellationToken ct = default)
            => await _context.BankReconciliations
                .Include(r => r.BankAccount)
                .Where(r => r.BankAccountId == bankAccountId)
                .OrderByDescending(r => r.ReconciliationDate)
                .ToListAsync(ct);

        public async Task<BankReconciliation> GetLatestCompletedAsync(int bankAccountId, CancellationToken ct = default)
            => await _context.BankReconciliations
                .Where(r => r.BankAccountId == bankAccountId && r.IsCompleted)
                .OrderByDescending(r => r.ReconciliationDate)
                .FirstOrDefaultAsync(ct);
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MarcoERP.Domain.Entities.Accounting;
using MarcoERP.Domain.Enums;
using MarcoERP.Domain.Interfaces;

namespace MarcoERP.Persistence.Repositories
{
    /// <summary>
    /// EF Core implementation of IFiscalYearRepository.
    /// </summary>
    public sealed class FiscalYearRepository : IFiscalYearRepository
    {
        private readonly MarcoDbContext _context;

        public FiscalYearRepository(MarcoDbContext context)
        {
            _context = context;
        }

        // ── IRepository<FiscalYear> ─────────────────────────────

        public async Task<FiscalYear> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.FiscalYears
                .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
        }

        public async Task<IReadOnlyList<FiscalYear>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.FiscalYears
                .OrderByDescending(f => f.Year)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(FiscalYear entity, CancellationToken cancellationToken = default)
        {
            await _context.FiscalYears.AddAsync(entity, cancellationToken);
        }

        public void Update(FiscalYear entity)
        {
            _context.FiscalYears.Update(entity);
        }

        public void Remove(FiscalYear entity)
        {
            _context.FiscalYears.Remove(entity);
        }

        // ── IFiscalYearRepository ───────────────────────────────

        public async Task<FiscalYear> GetByYearAsync(int year, CancellationToken cancellationToken = default)
        {
            return await _context.FiscalYears
                .FirstOrDefaultAsync(f => f.Year == year, cancellationToken);
        }

        public async Task<FiscalYear> GetActiveYearAsync(CancellationToken cancellationToken = default)
        {
            return await _context.FiscalYears
                .FirstOrDefaultAsync(f => f.Status == FiscalYearStatus.Active, cancellationToken);
        }

        public async Task<bool> YearExistsAsync(int year, CancellationToken cancellationToken = default)
        {
            return await _context.FiscalYears
                .AnyAsync(f => f.Year == year, cancellationToken);
        }

        public async Task<FiscalYear> GetWithPeriodsAsync(int fiscalYearId, CancellationToken cancellationToken = default)
        {
            return await _context.FiscalYears
                .Include(f => f.Periods)
                .FirstOrDefaultAsync(f => f.Id == fiscalYearId, cancellationToken);
        }
    }
}

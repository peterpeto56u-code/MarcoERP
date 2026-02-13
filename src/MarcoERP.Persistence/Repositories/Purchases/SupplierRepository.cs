using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MarcoERP.Domain.Entities.Purchases;
using MarcoERP.Domain.Interfaces.Purchases;

namespace MarcoERP.Persistence.Repositories.Purchases
{
    /// <summary>
    /// EF Core implementation of ISupplierRepository.
    /// Global soft-delete query filter applied by SupplierConfiguration.
    /// </summary>
    public sealed class SupplierRepository : ISupplierRepository
    {
        private readonly MarcoDbContext _context;

        public SupplierRepository(MarcoDbContext context)
        {
            _context = context;
        }

        // ── IRepository<Supplier> ───────────────────────────────

        public async Task<Supplier> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Suppliers
                .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        }

        public async Task<IReadOnlyList<Supplier>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Suppliers
                .OrderBy(s => s.Code)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(Supplier entity, CancellationToken cancellationToken = default)
        {
            await _context.Suppliers.AddAsync(entity, cancellationToken);
        }

        public void Update(Supplier entity)
        {
            _context.Suppliers.Update(entity);
        }

        public void Remove(Supplier entity)
        {
            _context.Suppliers.Remove(entity);
        }

        // ── ISupplierRepository ─────────────────────────────────

        public async Task<Supplier> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
        {
            return await _context.Suppliers
                .FirstOrDefaultAsync(s => s.Code == code, cancellationToken);
        }

        public async Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default)
        {
            return await _context.Suppliers
                .AnyAsync(s => s.Code == code, cancellationToken);
        }

        public async Task<bool> CodeExistsForOtherAsync(string code, int excludeId, CancellationToken cancellationToken = default)
        {
            return await _context.Suppliers
                .AnyAsync(s => s.Code == code && s.Id != excludeId, cancellationToken);
        }

        public async Task<IReadOnlyList<Supplier>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllAsync(cancellationToken);

            var pattern = $"%{searchTerm.Trim()}%";
            return await _context.Suppliers
                .Where(s => EF.Functions.Like(s.Code, pattern)
                          || EF.Functions.Like(s.NameAr, pattern)
                          || (s.NameEn != null && EF.Functions.Like(s.NameEn, pattern))
                          || (s.Phone != null && EF.Functions.Like(s.Phone, pattern))
                          || (s.Mobile != null && EF.Functions.Like(s.Mobile, pattern)))
                .OrderBy(s => s.Code)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Supplier>> GetActiveAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Suppliers
                .Where(s => s.IsActive)
                .OrderBy(s => s.Code)
                .ToListAsync(cancellationToken);
        }

        public async Task<string> GetNextCodeAsync(CancellationToken cancellationToken = default)
        {
            // Auto-generate: SUP-0001, SUP-0002, ...
            var lastCode = await _context.Suppliers
                .IgnoreQueryFilters()
                .Where(s => s.Code.StartsWith("SUP-"))
                .OrderByDescending(s => s.Code)
                .Select(s => s.Code)
                .FirstOrDefaultAsync(cancellationToken);

            if (lastCode == null)
                return "SUP-0001";

            var numPart = lastCode.Replace("SUP-", "");
            if (int.TryParse(numPart, out var num))
                return $"SUP-{(num + 1):D4}";

            return $"SUP-{1:D4}";
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MarcoERP.Domain.Entities.Purchases;
using MarcoERP.Domain.Enums;
using MarcoERP.Domain.Interfaces.Purchases;
using MarcoERP.Application.Interfaces;

namespace MarcoERP.Persistence.Repositories.Purchases
{
    /// <summary>
    /// EF Core implementation of IPurchaseReturnRepository.
    /// PurchaseReturn is not soft-deletable — only Draft returns can be hard-deleted.
    /// </summary>
    public sealed class PurchaseReturnRepository : IPurchaseReturnRepository
    {
        private readonly MarcoDbContext _context;
        private readonly IDateTimeProvider _dateTime;

        public PurchaseReturnRepository(MarcoDbContext context, IDateTimeProvider dateTime)
        {
            _context = context;
            _dateTime = dateTime;
        }

        // ── IRepository<PurchaseReturn> ─────────────────────────

        public async Task<PurchaseReturn> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.PurchaseReturns
                .Include(pr => pr.Supplier)
                .FirstOrDefaultAsync(pr => pr.Id == id, cancellationToken);
        }

        public async Task<IReadOnlyList<PurchaseReturn>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.PurchaseReturns
                .Include(pr => pr.Supplier)
                .OrderByDescending(pr => pr.ReturnDate)
                .ThenByDescending(pr => pr.ReturnNumber)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(PurchaseReturn entity, CancellationToken cancellationToken = default)
        {
            await _context.PurchaseReturns.AddAsync(entity, cancellationToken);
        }

        public void Update(PurchaseReturn entity)
        {
            _context.PurchaseReturns.Update(entity);
        }

        public void Remove(PurchaseReturn entity)
        {
            _context.PurchaseReturns.Remove(entity);
        }

        // ── IPurchaseReturnRepository ───────────────────────────

        public async Task<PurchaseReturn> GetWithLinesAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.PurchaseReturns
                .Include(pr => pr.Supplier)
                .Include(pr => pr.OriginalInvoice)
                .Include(pr => pr.Lines)
                .FirstOrDefaultAsync(pr => pr.Id == id, cancellationToken);
        }

        public async Task<PurchaseReturn> GetByNumberAsync(string returnNumber, CancellationToken cancellationToken = default)
        {
            return await _context.PurchaseReturns
                .Include(pr => pr.Supplier)
                .FirstOrDefaultAsync(pr => pr.ReturnNumber == returnNumber, cancellationToken);
        }

        public async Task<bool> NumberExistsAsync(string returnNumber, CancellationToken cancellationToken = default)
        {
            return await _context.PurchaseReturns
                .AnyAsync(pr => pr.ReturnNumber == returnNumber, cancellationToken);
        }

        public async Task<IReadOnlyList<PurchaseReturn>> GetByStatusAsync(InvoiceStatus status, CancellationToken cancellationToken = default)
        {
            return await _context.PurchaseReturns
                .Include(pr => pr.Supplier)
                .Where(pr => pr.Status == status)
                .OrderByDescending(pr => pr.ReturnDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<PurchaseReturn>> GetBySupplierAsync(int supplierId, CancellationToken cancellationToken = default)
        {
            return await _context.PurchaseReturns
                .Include(pr => pr.Supplier)
                .Where(pr => pr.SupplierId == supplierId)
                .OrderByDescending(pr => pr.ReturnDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<PurchaseReturn>> GetByOriginalInvoiceAsync(int invoiceId, CancellationToken cancellationToken = default)
        {
            return await _context.PurchaseReturns
                .Include(pr => pr.Supplier)
                .Where(pr => pr.OriginalInvoiceId == invoiceId)
                .OrderByDescending(pr => pr.ReturnDate)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Generates the next return number in format PR-YYYYMM-####.
        /// Example: PR-202602-0001, PR-202602-0002, ...
        /// </summary>
        public async Task<string> GetNextNumberAsync(CancellationToken cancellationToken = default)
        {
            var prefix = $"PR-{_dateTime.UtcNow:yyyyMM}-";

            var lastNumber = await _context.PurchaseReturns
                .Where(pr => pr.ReturnNumber.StartsWith(prefix))
                .OrderByDescending(pr => pr.ReturnNumber)
                .Select(pr => pr.ReturnNumber)
                .FirstOrDefaultAsync(cancellationToken);

            if (lastNumber == null)
                return $"{prefix}0001";

            var seqPart = lastNumber.Substring(prefix.Length);
            if (int.TryParse(seqPart, NumberStyles.None, CultureInfo.InvariantCulture, out var seq))
                return $"{prefix}{(seq + 1):D4}";

            return $"{prefix}0001";
        }
    }
}

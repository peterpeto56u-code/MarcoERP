using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MarcoERP.Domain.Entities.Treasury;
using MarcoERP.Domain.Enums;
using MarcoERP.Domain.Interfaces.Treasury;
using MarcoERP.Application.Interfaces;

namespace MarcoERP.Persistence.Repositories.Treasury
{
    /// <summary>
    /// EF Core implementation of ICashTransferRepository.
    /// Number format: CT-YYYYMM-####
    /// </summary>
    public sealed class CashTransferRepository : ICashTransferRepository
    {
        private readonly MarcoDbContext _context;
        private readonly IDateTimeProvider _dateTime;

        public CashTransferRepository(MarcoDbContext context, IDateTimeProvider dateTime)
        {
            _context = context;
            _dateTime = dateTime;
        }

        // ── IRepository<CashTransfer> ────────────────────────────

        public async Task<CashTransfer> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => await _context.CashTransfers
                .Include(ct2 => ct2.SourceCashbox)
                .Include(ct2 => ct2.TargetCashbox)
            .FirstOrDefaultAsync(ct2 => ct2.Id == id, cancellationToken);

        public async Task<IReadOnlyList<CashTransfer>> GetAllAsync(CancellationToken cancellationToken = default)
            => await _context.CashTransfers
                .Include(ct2 => ct2.SourceCashbox)
                .Include(ct2 => ct2.TargetCashbox)
                .OrderByDescending(ct2 => ct2.TransferDate)
                .ThenByDescending(ct2 => ct2.TransferNumber)
            .ToListAsync(cancellationToken);

        public async Task AddAsync(CashTransfer entity, CancellationToken cancellationToken = default)
            => await _context.CashTransfers.AddAsync(entity, cancellationToken);

        public void Update(CashTransfer entity) => _context.CashTransfers.Update(entity);
        public void Remove(CashTransfer entity) => _context.CashTransfers.Remove(entity);

        // ── ICashTransferRepository ──────────────────────────────

        public async Task<CashTransfer> GetWithDetailsAsync(int id, CancellationToken ct = default)
            => await _context.CashTransfers
                .Include(ct2 => ct2.SourceCashbox)
                .Include(ct2 => ct2.TargetCashbox)
                .FirstOrDefaultAsync(ct2 => ct2.Id == id, ct);

        public async Task<CashTransfer> GetByNumberAsync(string transferNumber, CancellationToken ct = default)
            => await _context.CashTransfers
                .Include(ct2 => ct2.SourceCashbox)
                .Include(ct2 => ct2.TargetCashbox)
                .FirstOrDefaultAsync(ct2 => ct2.TransferNumber == transferNumber, ct);

        public async Task<bool> NumberExistsAsync(string transferNumber, CancellationToken ct = default)
            => await _context.CashTransfers.AnyAsync(ct2 => ct2.TransferNumber == transferNumber, ct);

        public async Task<IReadOnlyList<CashTransfer>> GetByStatusAsync(InvoiceStatus status, CancellationToken ct = default)
            => await _context.CashTransfers
                .Include(ct2 => ct2.SourceCashbox)
                .Include(ct2 => ct2.TargetCashbox)
                .Where(ct2 => ct2.Status == status)
                .OrderByDescending(ct2 => ct2.TransferDate)
                .ToListAsync(ct);

        public async Task<IReadOnlyList<CashTransfer>> GetByCashboxAsync(int cashboxId, CancellationToken ct = default)
            => await _context.CashTransfers
                .Include(ct2 => ct2.SourceCashbox)
                .Include(ct2 => ct2.TargetCashbox)
                .Where(ct2 => ct2.SourceCashboxId == cashboxId || ct2.TargetCashboxId == cashboxId)
                .OrderByDescending(ct2 => ct2.TransferDate)
                .ToListAsync(ct);

        /// <summary>
        /// Generates the next transfer number in format CT-YYYYMM-####.
        /// Example: CT-202602-0001, CT-202602-0002, ...
        /// </summary>
        public async Task<string> GetNextNumberAsync(CancellationToken ct = default)
        {
            var prefix = $"CT-{_dateTime.UtcNow:yyyyMM}-";

            var lastNumber = await _context.CashTransfers
                .Where(ct2 => ct2.TransferNumber.StartsWith(prefix))
                .OrderByDescending(ct2 => ct2.TransferNumber)
                .Select(ct2 => ct2.TransferNumber)
                .FirstOrDefaultAsync(ct);

            if (lastNumber == null)
                return $"{prefix}0001";

            var seqPart = lastNumber.Substring(prefix.Length);
            if (int.TryParse(seqPart, NumberStyles.None, CultureInfo.InvariantCulture, out var seq))
                return $"{prefix}{(seq + 1):D4}";

            return $"{prefix}0001";
        }
    }
}

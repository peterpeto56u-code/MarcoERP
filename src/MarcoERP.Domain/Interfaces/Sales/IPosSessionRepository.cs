using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MarcoERP.Domain.Entities.Sales;
using MarcoERP.Domain.Enums;

namespace MarcoERP.Domain.Interfaces.Sales
{
    /// <summary>
    /// Repository contract for PosSession aggregate.
    /// </summary>
    public interface IPosSessionRepository : IRepository<PosSession>
    {
        /// <summary>Gets a session with all payments eagerly loaded.</summary>
        Task<PosSession> GetWithPaymentsAsync(int id, CancellationToken ct = default);

        /// <summary>Gets the current open session for a user.</summary>
        Task<PosSession> GetOpenSessionByUserAsync(int userId, CancellationToken ct = default);

        /// <summary>Checks if a user has an open session.</summary>
        Task<bool> HasOpenSessionAsync(int userId, CancellationToken ct = default);

        /// <summary>Gets sessions filtered by status.</summary>
        Task<IReadOnlyList<PosSession>> GetByStatusAsync(PosSessionStatus status, CancellationToken ct = default);

        /// <summary>Gets sessions within a date range.</summary>
        Task<IReadOnlyList<PosSession>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate, CancellationToken ct = default);

        /// <summary>Generates the next session number (POS-YYYYMMDD-####).</summary>
        Task<string> GetNextSessionNumberAsync(CancellationToken ct = default);
    }
}

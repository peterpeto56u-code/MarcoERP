using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MarcoERP.Domain.Entities.Accounting;
using MarcoERP.Domain.Enums;

namespace MarcoERP.Domain.Interfaces
{
    /// <summary>
    /// Repository interface for JournalEntry entity.
    /// </summary>
    public interface IJournalEntryRepository : IRepository<JournalEntry>
    {
        /// <summary>Gets a journal entry with all its lines loaded.</summary>
        Task<JournalEntry> GetWithLinesAsync(int journalEntryId, CancellationToken cancellationToken = default);

        /// <summary>Gets journal entries by fiscal period.</summary>
        Task<IReadOnlyList<JournalEntry>> GetByPeriodAsync(int fiscalPeriodId, CancellationToken cancellationToken = default);

        /// <summary>Gets journal entries by status.</summary>
        Task<IReadOnlyList<JournalEntry>> GetByStatusAsync(JournalEntryStatus status, CancellationToken cancellationToken = default);

        /// <summary>Gets all draft journal entries in a specific fiscal year.</summary>
        Task<IReadOnlyList<JournalEntry>> GetDraftsByYearAsync(int fiscalYearId, CancellationToken cancellationToken = default);

        /// <summary>Checks if any journal entries reference the given account.</summary>
        Task<bool> HasEntriesForAccountAsync(int accountId, CancellationToken cancellationToken = default);

        /// <summary>Gets journal entries by date range.</summary>
        Task<IReadOnlyList<JournalEntry>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

        /// <summary>Gets all posted journal entry lines for a specific fiscal year (for year-end closing).</summary>
        Task<IReadOnlyList<JournalEntryLine>> GetPostedLinesByYearAsync(int fiscalYearId, CancellationToken cancellationToken = default);
    }
}

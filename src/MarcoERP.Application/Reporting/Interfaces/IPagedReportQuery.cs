using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MarcoERP.Application.Reporting.Models;

namespace MarcoERP.Application.Reporting.Interfaces
{
    /// <summary>
    /// Generic paged report data provider.
    /// Implemented per report type to supply server-side paged, filtered, sorted data.
    /// </summary>
    public interface IPagedReportQuery<TRow> where TRow : ReportRowBase
    {
        /// <summary>
        /// Executes a paged query with server-side filtering and sorting.
        /// </summary>
        /// <param name="filters">Active filter values.</param>
        /// <param name="sort">Current sort definition.</param>
        /// <param name="pageIndex">Zero-based page index.</param>
        /// <param name="pageSize">Number of rows per page.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Paged result with total count.</returns>
        Task<PagedResult<TRow>> GetPageAsync(
            IReadOnlyList<ActiveFilter> filters,
            SortDefinition sort,
            int pageIndex,
            int pageSize,
            CancellationToken ct = default);

        /// <summary>
        /// Computes KPI summary cards for the current filter set.
        /// Called once on generate, not on every page change.
        /// </summary>
        Task<IReadOnlyList<KpiCard>> GetKpisAsync(
            IReadOnlyList<ActiveFilter> filters,
            CancellationToken ct = default);

        /// <summary>
        /// Loads child rows for an expandable parent row.
        /// Returns empty list if the row is not expandable.
        /// </summary>
        Task<IReadOnlyList<TRow>> GetChildRowsAsync(
            int parentRowId,
            IReadOnlyList<ActiveFilter> filters,
            CancellationToken ct = default);
    }
}

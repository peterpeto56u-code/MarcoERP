using System.Threading;
using System.Threading.Tasks;
using MarcoERP.Domain.Entities.Accounting;

namespace MarcoERP.Domain.Interfaces
{
    /// <summary>
    /// Repository interface for FiscalYear entity.
    /// </summary>
    public interface IFiscalYearRepository : IRepository<FiscalYear>
    {
        /// <summary>Gets the fiscal year by calendar year number.</summary>
        Task<FiscalYear> GetByYearAsync(int year, CancellationToken cancellationToken = default);

        /// <summary>Gets the currently active fiscal year (FY-INV-03: only one).</summary>
        Task<FiscalYear> GetActiveYearAsync(CancellationToken cancellationToken = default);

        /// <summary>Checks if a fiscal year with the given year number exists.</summary>
        Task<bool> YearExistsAsync(int year, CancellationToken cancellationToken = default);

        /// <summary>Gets the fiscal year with all 12 periods loaded.</summary>
        Task<FiscalYear> GetWithPeriodsAsync(int fiscalYearId, CancellationToken cancellationToken = default);
    }
}

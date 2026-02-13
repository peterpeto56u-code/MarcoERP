using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MarcoERP.Domain.Entities.Sales;

namespace MarcoERP.Domain.Interfaces.Sales
{
    /// <summary>
    /// Repository interface for PriceList aggregate (includes tiers).
    /// </summary>
    public interface IPriceListRepository : IRepository<PriceList>
    {
        /// <summary>Gets a price list with all its tiers loaded.</summary>
        Task<PriceList> GetWithTiersAsync(int id, CancellationToken ct = default);

        /// <summary>Gets a price list by code.</summary>
        Task<PriceList> GetByCodeAsync(string code, CancellationToken ct = default);

        /// <summary>Checks if a code already exists.</summary>
        Task<bool> CodeExistsAsync(string code, CancellationToken ct = default);

        /// <summary>Gets next auto-generated code.</summary>
        Task<string> GetNextCodeAsync(CancellationToken ct = default);

        /// <summary>Gets all active price lists valid on a specific date.</summary>
        Task<IReadOnlyList<PriceList>> GetActiveListsAsync(DateTime date, CancellationToken ct = default);

        /// <summary>
        /// Gets the best price for a product from any active price list valid on the given date.
        /// Returns the lowest price across all matching tiers.
        /// </summary>
        Task<decimal?> GetBestPriceAsync(int productId, decimal quantity, DateTime date, CancellationToken ct = default);
    }
}

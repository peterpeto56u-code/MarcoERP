using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MarcoERP.Domain.Entities.Purchases;

namespace MarcoERP.Domain.Interfaces.Purchases
{
    /// <summary>
    /// Repository contract for Supplier aggregate operations.
    /// </summary>
    public interface ISupplierRepository : IRepository<Supplier>
    {
        /// <summary>Gets a supplier by its unique code.</summary>
        Task<Supplier> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

        /// <summary>Checks if a supplier code already exists.</summary>
        Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default);

        /// <summary>Checks if a supplier code exists for another supplier (exclude given id).</summary>
        Task<bool> CodeExistsForOtherAsync(string code, int excludeId, CancellationToken cancellationToken = default);

        /// <summary>Searches suppliers by name or code.</summary>
        Task<IReadOnlyList<Supplier>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);

        /// <summary>Gets all active suppliers.</summary>
        Task<IReadOnlyList<Supplier>> GetActiveAsync(CancellationToken cancellationToken = default);

        /// <summary>Gets the next auto-generated supplier code.</summary>
        Task<string> GetNextCodeAsync(CancellationToken cancellationToken = default);
    }
}

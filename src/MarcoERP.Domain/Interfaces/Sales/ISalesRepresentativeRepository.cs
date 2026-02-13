using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MarcoERP.Domain.Entities.Sales;

namespace MarcoERP.Domain.Interfaces.Sales
{
    /// <summary>
    /// Repository interface for SalesRepresentative aggregate.
    /// </summary>
    public interface ISalesRepresentativeRepository : IRepository<SalesRepresentative>
    {
        /// <summary>Gets a sales representative by their unique code.</summary>
        Task<SalesRepresentative> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

        /// <summary>Checks if a code is already in use.</summary>
        Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default);

        /// <summary>Checks if a code is used by another representative (excluding the given ID).</summary>
        Task<bool> CodeExistsForOtherAsync(string code, int excludeId, CancellationToken cancellationToken = default);

        /// <summary>Searches by code, name, phone, or mobile.</summary>
        Task<IReadOnlyList<SalesRepresentative>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);

        /// <summary>Gets all active sales representatives.</summary>
        Task<IReadOnlyList<SalesRepresentative>> GetActiveAsync(CancellationToken cancellationToken = default);

        /// <summary>Gets the next auto-generated code.</summary>
        Task<string> GetNextCodeAsync(CancellationToken cancellationToken = default);
    }
}

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MarcoERP.Domain.Entities.Sales;

namespace MarcoERP.Domain.Interfaces.Sales
{
    /// <summary>
    /// Repository contract for Customer aggregate operations.
    /// </summary>
    public interface ICustomerRepository : IRepository<Customer>
    {
        /// <summary>Gets a customer by its unique code.</summary>
        Task<Customer> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

        /// <summary>Checks if a customer code already exists.</summary>
        Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default);

        /// <summary>Checks if a customer code exists for another customer (exclude given id).</summary>
        Task<bool> CodeExistsForOtherAsync(string code, int excludeId, CancellationToken cancellationToken = default);

        /// <summary>Searches customers by name or code.</summary>
        Task<IReadOnlyList<Customer>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);

        /// <summary>Gets all active customers.</summary>
        Task<IReadOnlyList<Customer>> GetActiveAsync(CancellationToken cancellationToken = default);

        /// <summary>Gets the next auto-generated customer code.</summary>
        Task<string> GetNextCodeAsync(CancellationToken cancellationToken = default);
    }
}

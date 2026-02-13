using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MarcoERP.Domain.Interfaces
{
    /// <summary>
    /// Generic repository interface. Domain layer defines the contract;
    /// Persistence layer provides the implementation.
    /// </summary>
    public interface IRepository<T> where T : class
    {
        /// <summary>Gets an entity by its primary key.</summary>
        Task<T> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>Gets all entities (non-deleted).</summary>
        Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);

        /// <summary>Adds a new entity.</summary>
        Task AddAsync(T entity, CancellationToken cancellationToken = default);

        /// <summary>Updates an existing entity.</summary>
        void Update(T entity);

        /// <summary>Removes an entity from the context.</summary>
        void Remove(T entity);
    }
}

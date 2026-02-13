using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MarcoERP.Domain.Entities.Inventory;

namespace MarcoERP.Domain.Interfaces.Inventory
{
    /// <summary>
    /// Repository contract for Unit entity.
    /// </summary>
    public interface IUnitRepository : IRepository<Unit>
    {
        /// <summary>Checks if a unit name already exists.</summary>
        Task<bool> NameExistsAsync(string nameAr, int? excludeId = null, CancellationToken ct = default);

        /// <summary>Gets all active units.</summary>
        Task<IReadOnlyList<Unit>> GetActiveUnitsAsync(CancellationToken ct = default);
    }
}

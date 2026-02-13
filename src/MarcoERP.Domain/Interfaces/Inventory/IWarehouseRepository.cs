using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MarcoERP.Domain.Entities.Inventory;

namespace MarcoERP.Domain.Interfaces.Inventory
{
    /// <summary>
    /// Repository contract for Warehouse entity.
    /// </summary>
    public interface IWarehouseRepository : IRepository<Warehouse>
    {
        /// <summary>Checks if a warehouse code already exists.</summary>
        Task<bool> CodeExistsAsync(string code, int? excludeId = null, CancellationToken ct = default);

        /// <summary>Gets the default warehouse.</summary>
        Task<Warehouse> GetDefaultAsync(CancellationToken ct = default);

        /// <summary>Gets all active warehouses.</summary>
        Task<IReadOnlyList<Warehouse>> GetActiveWarehousesAsync(CancellationToken ct = default);
    }
}

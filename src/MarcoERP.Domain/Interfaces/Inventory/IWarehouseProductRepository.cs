using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MarcoERP.Domain.Entities.Inventory;
using MarcoERP.Domain.Enums;

namespace MarcoERP.Domain.Interfaces.Inventory
{
    /// <summary>
    /// Repository contract for WarehouseProduct (stock balance) entity.
    /// </summary>
    public interface IWarehouseProductRepository : IRepository<WarehouseProduct>
    {
        /// <summary>Gets the stock record for a specific product in a specific warehouse.</summary>
        Task<WarehouseProduct> GetAsync(int warehouseId, int productId, CancellationToken ct = default);

        /// <summary>Gets or creates a stock record. Returns existing record or creates new with 0 quantity.</summary>
        Task<WarehouseProduct> GetOrCreateAsync(int warehouseId, int productId, CancellationToken ct = default);

        /// <summary>Gets total stock of a product across all warehouses (in base units).</summary>
        Task<decimal> GetTotalStockAsync(int productId, CancellationToken ct = default);

        /// <summary>Gets all stock records for a product across warehouses.</summary>
        Task<IReadOnlyList<WarehouseProduct>> GetByProductAsync(int productId, CancellationToken ct = default);

        /// <summary>Gets all stock records for a warehouse.</summary>
        Task<IReadOnlyList<WarehouseProduct>> GetByWarehouseAsync(int warehouseId, CancellationToken ct = default);

        /// <summary>Gets products below minimum stock level.</summary>
        Task<IReadOnlyList<WarehouseProduct>> GetBelowMinimumStockAsync(CancellationToken ct = default);
    }
}

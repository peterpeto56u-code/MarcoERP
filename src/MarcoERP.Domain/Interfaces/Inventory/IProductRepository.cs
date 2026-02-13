using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MarcoERP.Domain.Entities.Inventory;

namespace MarcoERP.Domain.Interfaces.Inventory
{
    /// <summary>
    /// Repository contract for Product entity.
    /// </summary>
    public interface IProductRepository : IRepository<Product>
    {
        /// <summary>Gets a product by ID with all its ProductUnits eagerly loaded.</summary>
        Task<Product> GetByIdWithUnitsAsync(int id, CancellationToken ct = default);

        /// <summary>Gets a product by its unique code.</summary>
        Task<Product> GetByCodeAsync(string code, CancellationToken ct = default);

        /// <summary>Gets all products in a specific category.</summary>
        Task<IReadOnlyList<Product>> GetByCategoryAsync(int categoryId, CancellationToken ct = default);

        /// <summary>Checks if a product code already exists.</summary>
        Task<bool> CodeExistsAsync(string code, int? excludeId = null, CancellationToken ct = default);

        /// <summary>Searches products by name or code (for popup search).</summary>
        Task<IReadOnlyList<Product>> SearchAsync(string searchTerm, CancellationToken ct = default);

        /// <summary>Gets all active products with their units.</summary>
        Task<IReadOnlyList<Product>> GetAllWithUnitsAsync(CancellationToken ct = default);

        /// <summary>Gets a product by barcode (searches both product and unit barcodes).</summary>
        Task<Product> GetByBarcodeAsync(string barcode, CancellationToken ct = default);

        /// <summary>Generates the next sequential product code (PRD-0001, PRD-0002, …).</summary>
        Task<string> GetNextCodeAsync(CancellationToken ct = default);
    }
}

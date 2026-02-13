using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MarcoERP.Domain.Entities.Inventory;

namespace MarcoERP.Domain.Interfaces.Inventory
{
    /// <summary>
    /// Repository contract for Category entity.
    /// </summary>
    public interface ICategoryRepository : IRepository<Category>
    {
        /// <summary>Gets all root categories (Level = 1).</summary>
        Task<IReadOnlyList<Category>> GetRootCategoriesAsync(CancellationToken ct = default);

        /// <summary>Gets children of a specific category.</summary>
        Task<IReadOnlyList<Category>> GetChildrenAsync(int parentId, CancellationToken ct = default);

        /// <summary>Checks if a category name already exists at the same level under the same parent.</summary>
        Task<bool> NameExistsAsync(string nameAr, int? parentId, int? excludeId = null, CancellationToken ct = default);
    }
}

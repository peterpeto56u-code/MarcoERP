using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MarcoERP.Application.Common;
using MarcoERP.Application.DTOs.Purchases;

namespace MarcoERP.Application.Interfaces.Purchases
{
    /// <summary>
    /// Application service contract for Supplier CRUD operations.
    /// </summary>
    public interface ISupplierService
    {
        /// <summary>Gets all suppliers.</summary>
        Task<ServiceResult<IReadOnlyList<SupplierDto>>> GetAllAsync(CancellationToken cancellationToken = default);

        /// <summary>Gets a supplier by ID.</summary>
        Task<ServiceResult<SupplierDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>Searches suppliers by name or code.</summary>
        Task<ServiceResult<IReadOnlyList<SupplierSearchResultDto>>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);

        /// <summary>Gets the next auto-generated supplier code.</summary>
        Task<ServiceResult<string>> GetNextCodeAsync(CancellationToken cancellationToken = default);

        /// <summary>Creates a new supplier.</summary>
        Task<ServiceResult<SupplierDto>> CreateAsync(CreateSupplierDto dto, CancellationToken cancellationToken = default);

        /// <summary>Updates an existing supplier.</summary>
        Task<ServiceResult<SupplierDto>> UpdateAsync(UpdateSupplierDto dto, CancellationToken cancellationToken = default);

        /// <summary>Activates a supplier.</summary>
        Task<ServiceResult> ActivateAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>Deactivates a supplier.</summary>
        Task<ServiceResult> DeactivateAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>Soft-deletes a supplier.</summary>
        Task<ServiceResult> DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}

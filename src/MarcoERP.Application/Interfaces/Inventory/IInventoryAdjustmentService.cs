using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MarcoERP.Application.Common;
using MarcoERP.Application.DTOs.Inventory;

namespace MarcoERP.Application.Interfaces.Inventory
{
    /// <summary>
    /// Application service for inventory adjustments.
    /// </summary>
    public interface IInventoryAdjustmentService
    {
        Task<ServiceResult<IReadOnlyList<InventoryAdjustmentListDto>>> GetAllAsync(CancellationToken ct = default);
        Task<ServiceResult<InventoryAdjustmentDto>> GetByIdAsync(int id, CancellationToken ct = default);
        Task<ServiceResult<string>> GetNextNumberAsync(CancellationToken ct = default);
        Task<ServiceResult<InventoryAdjustmentDto>> CreateAsync(CreateInventoryAdjustmentDto dto, CancellationToken ct = default);
        Task<ServiceResult<InventoryAdjustmentDto>> UpdateAsync(UpdateInventoryAdjustmentDto dto, CancellationToken ct = default);
        Task<ServiceResult<InventoryAdjustmentDto>> PostAsync(int id, CancellationToken ct = default);
        Task<ServiceResult> CancelAsync(int id, CancellationToken ct = default);
        Task<ServiceResult> DeleteDraftAsync(int id, CancellationToken ct = default);
    }
}

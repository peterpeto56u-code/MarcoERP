using MarcoERP.Application.DTOs.Inventory;
using MarcoERP.Domain.Entities.Inventory;

namespace MarcoERP.Application.Mappers.Inventory
{
    /// <summary>Manual mapper for Warehouse entity ↔ DTOs.</summary>
    public static class WarehouseMapper
    {
        public static WarehouseDto ToDto(Warehouse entity)
        {
            if (entity == null) return null;

            return new WarehouseDto
            {
                Id = entity.Id,
                Code = entity.Code,
                NameAr = entity.NameAr,
                NameEn = entity.NameEn,
                Address = entity.Address,
                Phone = entity.Phone,
                AccountId = entity.AccountId,
                IsActive = entity.IsActive,
                IsDefault = entity.IsDefault
            };
        }

        public static StockBalanceDto ToStockDto(WarehouseProduct wp)
        {
            if (wp == null) return null;

            return new StockBalanceDto
            {
                WarehouseId = wp.WarehouseId,
                WarehouseName = wp.Warehouse?.NameAr,
                ProductId = wp.ProductId,
                ProductName = wp.Product?.NameAr,
                ProductCode = wp.Product?.Code,
                Quantity = wp.Quantity,
                BaseUnitName = wp.Product?.BaseUnit?.NameAr,
                MinimumStock = wp.Product?.MinimumStock ?? 0
            };
        }
    }
}

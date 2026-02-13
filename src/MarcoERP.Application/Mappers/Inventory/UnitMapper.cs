using MarcoERP.Application.DTOs.Inventory;
using MarcoERP.Domain.Entities.Inventory;

namespace MarcoERP.Application.Mappers.Inventory
{
    /// <summary>Manual mapper for Unit entity ↔ DTOs.</summary>
    public static class UnitMapper
    {
        public static UnitDto ToDto(Unit entity)
        {
            if (entity == null) return null;

            return new UnitDto
            {
                Id = entity.Id,
                NameAr = entity.NameAr,
                NameEn = entity.NameEn,
                AbbreviationAr = entity.AbbreviationAr,
                AbbreviationEn = entity.AbbreviationEn,
                IsActive = entity.IsActive
            };
        }
    }
}

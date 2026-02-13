using MarcoERP.Application.DTOs.Inventory;
using MarcoERP.Domain.Entities.Inventory;

namespace MarcoERP.Application.Mappers.Inventory
{
    /// <summary>Manual mapper for Category entity ↔ DTOs.</summary>
    public static class CategoryMapper
    {
        public static CategoryDto ToDto(Category entity)
        {
            if (entity == null) return null;

            return new CategoryDto
            {
                Id = entity.Id,
                NameAr = entity.NameAr,
                NameEn = entity.NameEn,
                ParentCategoryId = entity.ParentCategoryId,
                ParentCategoryName = entity.ParentCategory?.NameAr,
                Level = entity.Level,
                IsActive = entity.IsActive,
                Description = entity.Description
            };
        }
    }
}

using MarcoERP.Application.DTOs.Sales;
using MarcoERP.Domain.Entities.Sales;

namespace MarcoERP.Application.Mappers.Sales
{
    /// <summary>
    /// Manual static mapper for SalesRepresentative entity ↔ DTOs.
    /// </summary>
    public static class SalesRepresentativeMapper
    {
        public static SalesRepresentativeDto ToDto(SalesRepresentative entity)
        {
            if (entity == null) return null;
            return new SalesRepresentativeDto
            {
                Id = entity.Id,
                Code = entity.Code,
                NameAr = entity.NameAr,
                NameEn = entity.NameEn,
                Phone = entity.Phone,
                Mobile = entity.Mobile,
                Email = entity.Email,
                CommissionRate = entity.CommissionRate,
                CommissionBasedOn = (int)entity.CommissionBasedOn,
                IsActive = entity.IsActive,
                Notes = entity.Notes
            };
        }

        public static SalesRepresentativeSearchResultDto ToSearchResult(SalesRepresentative entity)
        {
            if (entity == null) return null;
            return new SalesRepresentativeSearchResultDto
            {
                Id = entity.Id,
                Code = entity.Code,
                NameAr = entity.NameAr,
                Phone = entity.Phone,
                CommissionRate = entity.CommissionRate,
                IsActive = entity.IsActive
            };
        }
    }
}

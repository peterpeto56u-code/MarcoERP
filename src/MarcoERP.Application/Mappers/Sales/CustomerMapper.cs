using MarcoERP.Application.DTOs.Sales;
using MarcoERP.Domain.Entities.Sales;

namespace MarcoERP.Application.Mappers.Sales
{
    /// <summary>
    /// Manual mapper for Customer entity ↔ DTOs.
    /// No AutoMapper — per governance ARCHITECTURE §7.
    /// </summary>
    public static class CustomerMapper
    {
        /// <summary>Maps Customer entity → CustomerDto.</summary>
        public static CustomerDto ToDto(Customer entity)
        {
            if (entity == null) return null;

            return new CustomerDto
            {
                Id = entity.Id,
                Code = entity.Code,
                NameAr = entity.NameAr,
                NameEn = entity.NameEn,
                Phone = entity.Phone,
                Mobile = entity.Mobile,
                Address = entity.Address,
                City = entity.City,
                TaxNumber = entity.TaxNumber,
                PreviousBalance = entity.PreviousBalance,
                CreditLimit = entity.CreditLimit,
                DaysAllowed = entity.DaysAllowed,
                BlockedOnOverdue = entity.BlockedOnOverdue,
                PriceListId = entity.PriceListId,
                PriceListName = entity.PriceList?.NameAr,
                IsActive = entity.IsActive,
                Notes = entity.Notes,
                AccountId = entity.AccountId
            };
        }

        /// <summary>Maps Customer entity → CustomerSearchResultDto.</summary>
        public static CustomerSearchResultDto ToSearchResult(Customer entity)
        {
            if (entity == null) return null;

            return new CustomerSearchResultDto
            {
                Id = entity.Id,
                Code = entity.Code,
                NameAr = entity.NameAr,
                Phone = entity.Phone,
                IsActive = entity.IsActive
            };
        }
    }
}

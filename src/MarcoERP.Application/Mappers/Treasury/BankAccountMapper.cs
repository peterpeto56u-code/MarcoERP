using MarcoERP.Application.DTOs.Treasury;
using MarcoERP.Domain.Entities.Treasury;

namespace MarcoERP.Application.Mappers.Treasury
{
    /// <summary>Manual mapper for BankAccount entity → DTO.</summary>
    public static class BankAccountMapper
    {
        public static BankAccountDto ToDto(BankAccount entity)
        {
            if (entity == null) return null;

            return new BankAccountDto
            {
                Id = entity.Id,
                Code = entity.Code,
                NameAr = entity.NameAr,
                NameEn = entity.NameEn,
                BankName = entity.BankName,
                AccountNumber = entity.AccountNumber,
                IBAN = entity.IBAN,
                AccountId = entity.AccountId,
                IsActive = entity.IsActive,
                IsDefault = entity.IsDefault
            };
        }
    }
}

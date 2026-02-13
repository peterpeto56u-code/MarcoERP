using MarcoERP.Application.DTOs.Accounting;
using MarcoERP.Domain.Entities.Accounting;

namespace MarcoERP.Application.Mappers.Accounting
{
    /// <summary>
    /// Manual mapper between Account domain entity and Account DTOs.
    /// No AutoMapper dependency — explicit, auditable mapping.
    /// </summary>
    public static class AccountMapper
    {
        /// <summary>
        /// Maps an Account entity to AccountDto.
        /// Optionally includes parent info if parent is loaded.
        /// </summary>
        public static AccountDto ToDto(Account entity)
        {
            if (entity == null) return null;

            return new AccountDto
            {
                Id = entity.Id,
                AccountCode = entity.AccountCode,
                AccountNameAr = entity.AccountNameAr,
                AccountNameEn = entity.AccountNameEn,
                AccountType = entity.AccountType,
                NormalBalance = entity.NormalBalance,
                ParentAccountId = entity.ParentAccountId,
                ParentAccountCode = entity.ParentAccount?.AccountCode,
                ParentAccountNameAr = entity.ParentAccount?.AccountNameAr,
                Level = entity.Level,
                IsLeaf = entity.IsLeaf,
                AllowPosting = entity.AllowPosting,
                IsActive = entity.IsActive,
                IsSystemAccount = entity.IsSystemAccount,
                CurrencyCode = entity.CurrencyCode,
                Description = entity.Description,
                HasPostings = entity.HasPostings,
                RowVersion = entity.RowVersion
            };
        }

        /// <summary>
        /// Maps an Account entity to a tree node DTO (for hierarchy display).
        /// Children are NOT populated here — use BuildTree for full hierarchy.
        /// </summary>
        public static AccountTreeNodeDto ToTreeNode(Account entity)
        {
            if (entity == null) return null;

            return new AccountTreeNodeDto
            {
                Id = entity.Id,
                AccountCode = entity.AccountCode,
                AccountNameAr = entity.AccountNameAr,
                AccountNameEn = entity.AccountNameEn,
                AccountTypeName = entity.AccountType.ToString(),
                Level = entity.Level,
                IsLeaf = entity.IsLeaf,
                AllowPosting = entity.AllowPosting,
                IsActive = entity.IsActive,
                IsSystemAccount = entity.IsSystemAccount
            };
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using MarcoERP.Application.DTOs.Treasury;
using MarcoERP.Domain.Entities.Treasury;

namespace MarcoERP.Application.Mappers.Treasury
{
    /// <summary>Manual mapper for BankReconciliation entity → DTO.</summary>
    public static class BankReconciliationMapper
    {
        public static BankReconciliationDto ToDto(BankReconciliation entity)
        {
            if (entity == null) return null;

            return new BankReconciliationDto
            {
                Id = entity.Id,
                BankAccountId = entity.BankAccountId,
                BankAccountName = entity.BankAccount?.NameAr,
                ReconciliationDate = entity.ReconciliationDate,
                StatementBalance = entity.StatementBalance,
                SystemBalance = entity.SystemBalance,
                Difference = entity.Difference,
                IsCompleted = entity.IsCompleted,
                Notes = entity.Notes,
                Items = entity.Items?.Select(ToItemDto).ToList() ?? new List<BankReconciliationItemDto>()
            };
        }

        public static BankReconciliationItemDto ToItemDto(BankReconciliationItem item)
        {
            if (item == null) return null;

            return new BankReconciliationItemDto
            {
                Id = item.Id,
                BankReconciliationId = item.BankReconciliationId,
                TransactionDate = item.TransactionDate,
                Description = item.Description,
                Amount = item.Amount,
                Reference = item.Reference,
                IsMatched = item.IsMatched,
                JournalEntryId = item.JournalEntryId
            };
        }
    }
}

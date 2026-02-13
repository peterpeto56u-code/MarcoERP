using MarcoERP.Application.DTOs.Treasury;
using MarcoERP.Domain.Entities.Treasury;

namespace MarcoERP.Application.Mappers.Treasury
{
    /// <summary>Manual mapper for CashTransfer entity ↔ DTOs.</summary>
    public static class CashTransferMapper
    {
        public static CashTransferDto ToDto(CashTransfer entity)
        {
            if (entity == null) return null;

            return new CashTransferDto
            {
                Id = entity.Id,
                TransferNumber = entity.TransferNumber,
                TransferDate = entity.TransferDate,
                SourceCashboxId = entity.SourceCashboxId,
                SourceCashboxName = entity.SourceCashbox?.NameAr,
                TargetCashboxId = entity.TargetCashboxId,
                TargetCashboxName = entity.TargetCashbox?.NameAr,
                Amount = entity.Amount,
                Description = entity.Description,
                Notes = entity.Notes,
                Status = entity.Status.ToString(),
                JournalEntryId = entity.JournalEntryId
            };
        }

        public static CashTransferListDto ToListDto(CashTransfer entity)
        {
            if (entity == null) return null;

            return new CashTransferListDto
            {
                Id = entity.Id,
                TransferNumber = entity.TransferNumber,
                TransferDate = entity.TransferDate,
                SourceCashboxName = entity.SourceCashbox?.NameAr,
                TargetCashboxName = entity.TargetCashbox?.NameAr,
                Amount = entity.Amount,
                Status = entity.Status.ToString()
            };
        }
    }
}

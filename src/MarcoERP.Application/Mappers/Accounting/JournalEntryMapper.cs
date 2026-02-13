using System.Collections.Generic;
using System.Linq;
using MarcoERP.Application.DTOs.Accounting;
using MarcoERP.Domain.Entities.Accounting;

namespace MarcoERP.Application.Mappers.Accounting
{
    /// <summary>
    /// Manual mapper between JournalEntry domain entity and JournalEntry DTOs.
    /// </summary>
    public static class JournalEntryMapper
    {
        /// <summary>
        /// Maps a JournalEntry entity (with lines) to JournalEntryDto.
        /// </summary>
        public static JournalEntryDto ToDto(JournalEntry entity)
        {
            if (entity == null) return null;

            return new JournalEntryDto
            {
                Id = entity.Id,
                JournalNumber = entity.JournalNumber,
                DraftCode = entity.DraftCode,
                JournalDate = entity.JournalDate,
                PostingDate = entity.PostingDate,
                Description = entity.Description,
                ReferenceNumber = entity.ReferenceNumber,
                Status = entity.Status,
                StatusName = GetStatusName(entity.Status),
                SourceType = entity.SourceType,
                SourceTypeName = GetSourceTypeName(entity.SourceType),
                SourceId = entity.SourceId,
                FiscalYearId = entity.FiscalYearId,
                FiscalPeriodId = entity.FiscalPeriodId,
                CostCenterId = entity.CostCenterId,
                ReversedEntryId = entity.ReversedEntryId,
                ReversalEntryId = entity.ReversalEntryId,
                AdjustedEntryId = entity.AdjustedEntryId,
                ReversalReason = entity.ReversalReason,
                PostedBy = entity.PostedBy,
                TotalDebit = entity.TotalDebit,
                TotalCredit = entity.TotalCredit,
                RowVersion = entity.RowVersion,
                Lines = entity.Lines?.Select(ToLineDto).ToList() ?? new List<JournalEntryLineDto>()
            };
        }

        /// <summary>
        /// Maps a JournalEntryLine entity to JournalEntryLineDto.
        /// </summary>
        public static JournalEntryLineDto ToLineDto(JournalEntryLine line)
        {
            if (line == null) return null;

            return new JournalEntryLineDto
            {
                Id = line.Id,
                LineNumber = line.LineNumber,
                AccountId = line.AccountId,
                // AccountCode and AccountNameAr populated by service if needed
                DebitAmount = line.DebitAmount,
                CreditAmount = line.CreditAmount,
                Description = line.Description,
                CostCenterId = line.CostCenterId,
                WarehouseId = line.WarehouseId
            };
        }

        /// <summary>
        /// Maps a PostJournalResultDto from a posted entry.
        /// </summary>
        public static PostJournalResultDto ToPostResult(JournalEntry entity)
        {
            if (entity == null) return null;

            return new PostJournalResultDto
            {
                JournalEntryId = entity.Id,
                JournalNumber = entity.JournalNumber,
                PostingDate = entity.PostingDate.GetValueOrDefault()
            };
        }

        private static string GetStatusName(Domain.Enums.JournalEntryStatus status)
        {
            return status switch
            {
                Domain.Enums.JournalEntryStatus.Draft => "مسودة",
                Domain.Enums.JournalEntryStatus.Posted => "مرحّل",
                Domain.Enums.JournalEntryStatus.Reversed => "معكوس",
                _ => status.ToString()
            };
        }

        private static string GetSourceTypeName(Domain.Enums.SourceType sourceType)
        {
            return sourceType switch
            {
                Domain.Enums.SourceType.Manual => "يدوي",
                Domain.Enums.SourceType.SalesInvoice => "فاتورة مبيعات",
                Domain.Enums.SourceType.PurchaseInvoice => "فاتورة مشتريات",
                Domain.Enums.SourceType.CashReceipt => "سند قبض",
                Domain.Enums.SourceType.CashPayment => "سند صرف",
                Domain.Enums.SourceType.Inventory => "حركة مخزنية",
                Domain.Enums.SourceType.Adjustment => "تسوية",
                Domain.Enums.SourceType.Opening => "أرصدة افتتاحية",
                Domain.Enums.SourceType.Closing => "قيد إقفال",
                _ => sourceType.ToString()
            };
        }
    }
}

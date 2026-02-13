using System;
using System.Collections.Generic;
using MarcoERP.Domain.Enums;

namespace MarcoERP.Application.DTOs.Accounting
{
    /// <summary>
    /// Read-only DTO representing a full journal entry with lines.
    /// </summary>
    public sealed class JournalEntryDto
    {
        public int Id { get; set; }
        public string JournalNumber { get; set; }
        public string DraftCode { get; set; }
        public DateTime JournalDate { get; set; }
        public DateTime? PostingDate { get; set; }
        public string Description { get; set; }
        public string ReferenceNumber { get; set; }
        public JournalEntryStatus Status { get; set; }
        public string StatusName { get; set; }
        public SourceType SourceType { get; set; }
        public string SourceTypeName { get; set; }
        public int? SourceId { get; set; }
        public int FiscalYearId { get; set; }
        public int FiscalPeriodId { get; set; }
        public int? CostCenterId { get; set; }
        public int? ReversedEntryId { get; set; }
        public int? ReversalEntryId { get; set; }
        public int? AdjustedEntryId { get; set; }
        public string ReversalReason { get; set; }
        public string PostedBy { get; set; }
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
        public byte[] RowVersion { get; set; }
        public List<JournalEntryLineDto> Lines { get; set; } = new();
    }
}

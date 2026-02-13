using System;

namespace MarcoERP.Application.DTOs.Reports
{
    /// <summary>
    /// DTO for a single row in the Account Statement report.
    /// </summary>
    public sealed class AccountStatementRowDto
    {
        public DateTime Date { get; set; }
        public string JournalNumber { get; set; }
        public string Description { get; set; }
        public string SourceTypeName { get; set; }
        public decimal DebitAmount { get; set; }
        public decimal CreditAmount { get; set; }
        public decimal RunningBalance { get; set; }
    }

    /// <summary>
    /// Wrapper DTO for the full Account Statement report including header info.
    /// </summary>
    public sealed class AccountStatementReportDto
    {
        public int AccountId { get; set; }
        public string AccountCode { get; set; }
        public string AccountNameAr { get; set; }
        public string AccountTypeName { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
        public decimal ClosingBalance { get; set; }
        public System.Collections.Generic.List<AccountStatementRowDto> Rows { get; set; } = new();
    }
}

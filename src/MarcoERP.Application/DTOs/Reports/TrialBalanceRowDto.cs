namespace MarcoERP.Application.DTOs.Reports
{
    /// <summary>
    /// DTO for a single row in the Trial Balance report.
    /// Each row represents one account's aggregated debit/credit totals.
    /// </summary>
    public sealed class TrialBalanceRowDto
    {
        public int AccountId { get; set; }
        public string AccountCode { get; set; }
        public string AccountNameAr { get; set; }
        public string AccountTypeName { get; set; }
        public int Level { get; set; }
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
        public decimal Balance { get; set; }
        public string BalanceSide { get; set; } // "مدين" or "دائن"
    }
}

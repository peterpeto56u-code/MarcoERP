namespace MarcoERP.Application.DTOs.Reports
{
    /// <summary>
    /// DTO for a single row in the Balance Sheet report.
    /// </summary>
    public sealed class BalanceSheetRowDto
    {
        public string AccountCode { get; set; }
        public string AccountNameAr { get; set; }
        public string AccountTypeName { get; set; }
        public int Level { get; set; }
        public decimal Balance { get; set; }
    }

    /// <summary>
    /// Full Balance Sheet report with grouped sections.
    /// </summary>
    public sealed class BalanceSheetDto
    {
        public System.Collections.Generic.List<BalanceSheetRowDto> AssetRows { get; set; } = new();
        public System.Collections.Generic.List<BalanceSheetRowDto> LiabilityRows { get; set; } = new();
        public System.Collections.Generic.List<BalanceSheetRowDto> EquityRows { get; set; } = new();

        public decimal TotalAssets { get; set; }
        public decimal TotalLiabilities { get; set; }
        public decimal TotalEquity { get; set; }
        public decimal RetainedEarnings { get; set; }
        public decimal TotalLiabilitiesAndEquity { get; set; }
    }
}

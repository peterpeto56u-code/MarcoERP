namespace MarcoERP.Application.DTOs.Reports
{
    /// <summary>
    /// DTO for a single row in the Income Statement (Profit and Loss).
    /// </summary>
    public sealed class IncomeStatementRowDto
    {
        public string AccountCode { get; set; }
        public string AccountNameAr { get; set; }
        public string AccountTypeName { get; set; }
        public decimal Amount { get; set; }
    }

    /// <summary>
    /// Full Income Statement report with grouped sections.
    /// </summary>
    public sealed class IncomeStatementDto
    {
        public System.Collections.Generic.List<IncomeStatementRowDto> RevenueRows { get; set; } = new();
        public System.Collections.Generic.List<IncomeStatementRowDto> CogsRows { get; set; } = new();
        public System.Collections.Generic.List<IncomeStatementRowDto> ExpenseRows { get; set; } = new();
        public System.Collections.Generic.List<IncomeStatementRowDto> OtherIncomeRows { get; set; } = new();
        public System.Collections.Generic.List<IncomeStatementRowDto> OtherExpenseRows { get; set; } = new();

        public decimal TotalRevenue { get; set; }
        public decimal TotalCogs { get; set; }
        public decimal GrossProfit { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal TotalOtherIncome { get; set; }
        public decimal TotalOtherExpenses { get; set; }
        public decimal NetProfit { get; set; }
    }
}

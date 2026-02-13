namespace MarcoERP.Domain.Enums
{
    /// <summary>
    /// Represents the natural (normal) balance side of an account.
    /// Derived from AccountType — never set independently.
    /// </summary>
    public enum NormalBalance
    {
        /// <summary>Asset, COGS, Expense, OtherExpense → increases by debit.</summary>
        Debit = 0,

        /// <summary>Liability, Equity, Revenue, OtherIncome → increases by credit.</summary>
        Credit = 1
    }
}

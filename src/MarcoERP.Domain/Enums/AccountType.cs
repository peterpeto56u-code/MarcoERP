namespace MarcoERP.Domain.Enums
{
    /// <summary>
    /// Classification of accounts in the chart of accounts.
    /// Determines code range, normal balance, and closing behavior.
    /// </summary>
    public enum AccountType
    {
        /// <summary>1xxx — Normal balance: Debit</summary>
        Asset = 0,

        /// <summary>2xxx — Normal balance: Credit</summary>
        Liability = 1,

        /// <summary>3xxx — Normal balance: Credit</summary>
        Equity = 2,

        /// <summary>4xxx — Normal balance: Credit</summary>
        Revenue = 3,

        /// <summary>5xxx — Normal balance: Debit. Closed to Retained Earnings at year-end.</summary>
        COGS = 4,

        /// <summary>6xxx — Normal balance: Debit. Closed to Retained Earnings at year-end.</summary>
        Expense = 5,

        /// <summary>7xxx — Normal balance: Credit. Closed to Retained Earnings at year-end.</summary>
        OtherIncome = 6,

        /// <summary>8xxx — Normal balance: Debit. Closed to Retained Earnings at year-end.</summary>
        OtherExpense = 7
    }
}

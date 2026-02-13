namespace MarcoERP.Domain.Enums
{
    /// <summary>
    /// Determines the basis for sales representative commission calculation.
    /// </summary>
    public enum CommissionBasis
    {
        /// <summary>Commission calculated as percentage of net sales amount.</summary>
        Sales = 0,

        /// <summary>Commission calculated as percentage of gross profit (Sales - COGS).</summary>
        Profit = 1
    }
}

namespace MarcoERP.Domain.Enums
{
    /// <summary>
    /// Lifecycle status of a fiscal year.
    /// Setup → Active → Closed (irreversible).
    /// Only ONE fiscal year may be Active at any time.
    /// </summary>
    public enum FiscalYearStatus
    {
        /// <summary>Year created but not yet activated. No posting allowed.</summary>
        Setup = 0,

        /// <summary>Year is active and open for posting.</summary>
        Active = 1,

        /// <summary>Year permanently closed. No further posting or changes.</summary>
        Closed = 2
    }
}

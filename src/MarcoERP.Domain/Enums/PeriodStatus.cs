namespace MarcoERP.Domain.Enums
{
    /// <summary>
    /// Lifecycle status of a fiscal period (month).
    /// Open → Locked. Unlock is admin-only and only for the most recent locked period.
    /// </summary>
    public enum PeriodStatus
    {
        /// <summary>Period is open for posting.</summary>
        Open = 0,

        /// <summary>Period is locked — no new postings allowed.</summary>
        Locked = 1
    }
}

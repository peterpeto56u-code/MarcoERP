namespace MarcoERP.Domain.Enums
{
    /// <summary>
    /// Lifecycle status of a POS session.
    /// Open → Closed.
    /// </summary>
    public enum PosSessionStatus
    {
        /// <summary>Session is active — sales can be recorded.</summary>
        Open = 0,

        /// <summary>Session has been closed and reconciled.</summary>
        Closed = 1
    }
}

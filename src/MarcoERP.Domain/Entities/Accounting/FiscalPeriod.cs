using System;
using MarcoERP.Domain.Entities.Common;
using MarcoERP.Domain.Enums;
using MarcoERP.Domain.Exceptions.Accounting;

namespace MarcoERP.Domain.Entities.Accounting
{
    /// <summary>
    /// Represents a monthly fiscal period within a fiscal year.
    /// 12 periods per year, locked sequentially (PER-01).
    /// Open → Locked. Unlock is admin-only for the most recent locked period (PER-05).
    /// </summary>
    public sealed class FiscalPeriod : AuditableEntity
    {
        // ── Constructors ────────────────────────────────────────

        /// <summary>EF Core only.</summary>
        private FiscalPeriod() { }

        /// <summary>
        /// Creates a new fiscal period.
        /// </summary>
        internal FiscalPeriod(int periodNumber, int year, int month, DateTime startDate, DateTime endDate)
        {
            // FP-INV-01
            if (periodNumber < 1 || periodNumber > 12)
                throw new FiscalPeriodDomainException("رقم الفترة يجب أن يكون بين 1 و 12.");

            if (month < 1 || month > 12)
                throw new FiscalPeriodDomainException("رقم الشهر يجب أن يكون بين 1 و 12.");

            PeriodNumber = periodNumber;
            Year = year;
            Month = month;
            StartDate = startDate;
            EndDate = endDate;
            Status = PeriodStatus.Open;
        }

        // ── Properties ──────────────────────────────────────────

        /// <summary>FK to parent FiscalYear.</summary>
        public int FiscalYearId { get; private set; }

        /// <summary>Period number: 1 through 12.</summary>
        public int PeriodNumber { get; private set; }

        /// <summary>Calendar year (denormalized for convenience).</summary>
        public int Year { get; private set; }

        /// <summary>Calendar month: 1 through 12.</summary>
        public int Month { get; private set; }

        /// <summary>First day of the month.</summary>
        public DateTime StartDate { get; private set; }

        /// <summary>Last day of the month.</summary>
        public DateTime EndDate { get; private set; }

        /// <summary>Open = 0, Locked = 1.</summary>
        public PeriodStatus Status { get; private set; }

        /// <summary>UTC timestamp of lock.</summary>
        public DateTime? LockedAt { get; private set; }

        /// <summary>User who locked the period.</summary>
        public string LockedBy { get; private set; }

        /// <summary>Admin-only unlock justification note (PER-06).</summary>
        public string UnlockReason { get; private set; }

        // ── Domain Methods ──────────────────────────────────────

        /// <summary>
        /// Returns true if the period is open for posting.
        /// </summary>
        public bool IsOpen => Status == PeriodStatus.Open;

        /// <summary>
        /// Returns true if the date falls within this period.
        /// FP-INV-02: StartDate and EndDate correctly represent the calendar month.
        /// </summary>
        public bool ContainsDate(DateTime date)
        {
            return date.Date >= StartDate.Date && date.Date <= EndDate.Date;
        }

        /// <summary>
        /// Locks the period, preventing further postings.
        /// FP-INV-03: Sequential locking enforced by caller (Application layer checks previous period is locked).
        /// FP-INV-04: Caller verifies no pending drafts in this period.
        /// </summary>
        public void Lock(string lockedBy, DateTime lockedAt)
        {
            if (Status != PeriodStatus.Open)
                throw new FiscalPeriodDomainException("الفترة مقفلة بالفعل.");

            if (string.IsNullOrWhiteSpace(lockedBy))
                throw new FiscalPeriodDomainException("اسم المستخدم مطلوب لقفل الفترة.");

            Status = PeriodStatus.Locked;
            LockedAt = lockedAt;
            LockedBy = lockedBy.Trim();
        }

        /// <summary>
        /// Unlocks the period (admin-only).
        /// FP-INV-05: Only the most recent locked period can be unlocked. Caller enforces.
        /// PER-06: Mandatory unlock justification.
        /// </summary>
        public void Unlock(string reason)
        {
            if (Status != PeriodStatus.Locked)
                throw new FiscalPeriodDomainException("الفترة مفتوحة بالفعل.");

            if (string.IsNullOrWhiteSpace(reason))
                throw new FiscalPeriodDomainException("سبب فتح الفترة مطلوب — يتم تسجيله في سجل المراجعة.");

            Status = PeriodStatus.Open;
            LockedAt = null;
            LockedBy = null;
            UnlockReason = reason.Trim();
        }
    }
}

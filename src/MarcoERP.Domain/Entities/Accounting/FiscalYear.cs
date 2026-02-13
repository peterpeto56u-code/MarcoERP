using System;
using System.Collections.Generic;
using System.Linq;
using MarcoERP.Domain.Entities.Common;
using MarcoERP.Domain.Enums;
using MarcoERP.Domain.Exceptions.Accounting;

namespace MarcoERP.Domain.Entities.Accounting
{
    /// <summary>
    /// Represents a fiscal (financial) year.
    /// Lifecycle: Setup → Active → Closed (irreversible).
    /// Only ONE FiscalYear may be Active at any time (FY-INV-03).
    /// Always calendar-year: Jan 1 → Dec 31 (FY-INV-01, FY-INV-02).
    /// </summary>
    public sealed class FiscalYear : AuditableEntity
    {
        private readonly List<FiscalPeriod> _periods = new();

        // ── Constructors ────────────────────────────────────────

        /// <summary>EF Core only.</summary>
        private FiscalYear() { }

        /// <summary>
        /// Creates a new fiscal year with 12 monthly periods.
        /// FY-INV-01: StartDate = Jan 1. FY-INV-02: EndDate = Dec 31.
        /// FY-INV-04: Exactly 12 periods auto-created.
        /// </summary>
        public FiscalYear(int year)
        {
            if (year < 2000 || year > 2100)
                throw new FiscalYearDomainException("السنة المالية يجب أن تكون بين 2000 و 2100.");

            Year = year;
            StartDate = new DateTime(year, 1, 1);
            EndDate = new DateTime(year, 12, 31);
            Status = FiscalYearStatus.Setup;

            // Auto-create 12 monthly periods
            for (int month = 1; month <= 12; month++)
            {
                var startDate = new DateTime(year, month, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);
                _periods.Add(new FiscalPeriod(month, year, month, startDate, endDate));
            }
        }

        // ── Properties ──────────────────────────────────────────

        /// <summary>Calendar year (e.g., 2026). Unique across all fiscal years.</summary>
        public int Year { get; private set; }

        /// <summary>Always January 1 of Year.</summary>
        public DateTime StartDate { get; private set; }

        /// <summary>Always December 31 of Year.</summary>
        public DateTime EndDate { get; private set; }

        /// <summary>Setup = 0, Active = 1, Closed = 2.</summary>
        public FiscalYearStatus Status { get; private set; }

        /// <summary>UTC timestamp of closure.</summary>
        public DateTime? ClosedAt { get; private set; }

        /// <summary>User who closed the year.</summary>
        public string ClosedBy { get; private set; }

        /// <summary>Navigation to 12 fiscal periods.</summary>
        public IReadOnlyCollection<FiscalPeriod> Periods => _periods.AsReadOnly();

        // ── Domain Methods ──────────────────────────────────────

        /// <summary>
        /// Activates the fiscal year. Only one year can be active at a time.
        /// Caller (Application layer) must verify no other Active year exists (FY-INV-03).
        /// </summary>
        public void Activate()
        {
            if (Status != FiscalYearStatus.Setup)
                throw new FiscalYearDomainException("يمكن تفعيل السنة المالية من حالة الإعداد فقط.");

            if (_periods.Count != 12)
                throw new FiscalYearDomainException("السنة المالية يجب أن تحتوي على 12 فترة بالضبط.");

            Status = FiscalYearStatus.Active;
        }

        /// <summary>
        /// Closes the fiscal year permanently.
        /// FY-INV-06: All 12 periods must be Locked.
        /// FY-INV-08: Closure is irreversible.
        /// Caller must also verify trial balance and no pending drafts (Application layer).
        /// </summary>
        public void Close(string closedBy, DateTime closedAt)
        {
            if (Status != FiscalYearStatus.Active)
                throw new FiscalYearDomainException("يمكن إغلاق السنة المالية الفعالة فقط.");

            if (string.IsNullOrWhiteSpace(closedBy))
                throw new FiscalYearDomainException("اسم المستخدم مطلوب لإغلاق السنة المالية.");

            // FY-INV-06: All 12 periods must be Locked
            if (_periods.Any(p => p.Status != PeriodStatus.Locked))
                throw new FiscalYearDomainException("يجب قفل جميع الفترات الـ 12 قبل إغلاق السنة المالية.");

            Status = FiscalYearStatus.Closed;
            ClosedAt = closedAt;
            ClosedBy = closedBy.Trim();
        }

        /// <summary>
        /// Returns true if the year is active and can accept postings.
        /// </summary>
        public bool IsOpen => Status == FiscalYearStatus.Active;

        /// <summary>
        /// Returns true if the fiscal year contains the given date.
        /// </summary>
        public bool ContainsDate(DateTime date)
        {
            return date.Date >= StartDate.Date && date.Date <= EndDate.Date;
        }

        /// <summary>
        /// Gets the fiscal period for the given month (1–12).
        /// </summary>
        public FiscalPeriod GetPeriod(int month)
        {
            if (month < 1 || month > 12)
                throw new FiscalYearDomainException("رقم الشهر يجب أن يكون بين 1 و 12.");

            return _periods.FirstOrDefault(p => p.Month == month);
        }
    }
}

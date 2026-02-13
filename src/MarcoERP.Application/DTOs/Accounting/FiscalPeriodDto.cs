using MarcoERP.Domain.Enums;

namespace MarcoERP.Application.DTOs.Accounting
{
    /// <summary>
    /// Read-only DTO for a fiscal period within a fiscal year.
    /// </summary>
    public sealed class FiscalPeriodDto
    {
        public int Id { get; set; }
        public int FiscalYearId { get; set; }
        public int PeriodNumber { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public System.DateTime StartDate { get; set; }
        public System.DateTime EndDate { get; set; }
        public PeriodStatus Status { get; set; }
        public string StatusName { get; set; }
        public System.DateTime? LockedAt { get; set; }
        public string LockedBy { get; set; }
        public string UnlockReason { get; set; }
    }
}

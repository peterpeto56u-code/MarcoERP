using System.Collections.Generic;
using MarcoERP.Domain.Enums;

namespace MarcoERP.Application.DTOs.Accounting
{
    /// <summary>
    /// Read-only DTO for a fiscal year with its 12 periods.
    /// </summary>
    public sealed class FiscalYearDto
    {
        public int Id { get; set; }
        public int Year { get; set; }
        public System.DateTime StartDate { get; set; }
        public System.DateTime EndDate { get; set; }
        public FiscalYearStatus Status { get; set; }
        public string StatusName { get; set; }
        public System.DateTime? ClosedAt { get; set; }
        public string ClosedBy { get; set; }
        public List<FiscalPeriodDto> Periods { get; set; } = new();
    }
}

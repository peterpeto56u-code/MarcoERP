using System.Linq;
using MarcoERP.Application.DTOs.Accounting;
using MarcoERP.Domain.Entities.Accounting;

namespace MarcoERP.Application.Mappers.Accounting
{
    /// <summary>
    /// Manual mapper between FiscalYear/FiscalPeriod domain entities and DTOs.
    /// </summary>
    public static class FiscalYearMapper
    {
        /// <summary>
        /// Maps a FiscalYear entity (with periods) to FiscalYearDto.
        /// </summary>
        public static FiscalYearDto ToDto(FiscalYear entity)
        {
            if (entity == null) return null;

            return new FiscalYearDto
            {
                Id = entity.Id,
                Year = entity.Year,
                StartDate = entity.StartDate,
                EndDate = entity.EndDate,
                Status = entity.Status,
                StatusName = GetStatusName(entity.Status),
                ClosedAt = entity.ClosedAt,
                ClosedBy = entity.ClosedBy,
                Periods = entity.Periods?.Select(ToPeriodDto).OrderBy(p => p.PeriodNumber).ToList()
                          ?? new System.Collections.Generic.List<FiscalPeriodDto>()
            };
        }

        /// <summary>
        /// Maps a FiscalPeriod entity to FiscalPeriodDto.
        /// </summary>
        public static FiscalPeriodDto ToPeriodDto(FiscalPeriod entity)
        {
            if (entity == null) return null;

            return new FiscalPeriodDto
            {
                Id = entity.Id,
                FiscalYearId = entity.FiscalYearId,
                PeriodNumber = entity.PeriodNumber,
                Year = entity.Year,
                Month = entity.Month,
                StartDate = entity.StartDate,
                EndDate = entity.EndDate,
                Status = entity.Status,
                StatusName = GetPeriodStatusName(entity.Status),
                LockedAt = entity.LockedAt,
                LockedBy = entity.LockedBy,
                UnlockReason = entity.UnlockReason
            };
        }

        private static string GetStatusName(Domain.Enums.FiscalYearStatus status)
        {
            return status switch
            {
                Domain.Enums.FiscalYearStatus.Setup => "إعداد",
                Domain.Enums.FiscalYearStatus.Active => "فعّال",
                Domain.Enums.FiscalYearStatus.Closed => "مُغلق",
                _ => status.ToString()
            };
        }

        private static string GetPeriodStatusName(Domain.Enums.PeriodStatus status)
        {
            return status switch
            {
                Domain.Enums.PeriodStatus.Open => "مفتوح",
                Domain.Enums.PeriodStatus.Locked => "مُقفل",
                _ => status.ToString()
            };
        }
    }
}

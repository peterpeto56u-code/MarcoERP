namespace MarcoERP.Application.DTOs.Accounting
{
    /// <summary>
    /// DTO for creating a new fiscal year.
    /// The system auto-creates 12 periods and sets Start/End dates.
    /// </summary>
    public sealed class CreateFiscalYearDto
    {
        /// <summary>Calendar year (e.g., 2026). Must be unique and between 2000–2100.</summary>
        public int Year { get; set; }
    }
}

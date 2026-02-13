namespace MarcoERP.Application.DTOs.Accounting
{
    /// <summary>
    /// DTO for a single journal entry line (read-only display).
    /// </summary>
    public sealed class JournalEntryLineDto
    {
        public int Id { get; set; }
        public int LineNumber { get; set; }
        public int AccountId { get; set; }
        public string AccountCode { get; set; }
        public string AccountNameAr { get; set; }
        public decimal DebitAmount { get; set; }
        public decimal CreditAmount { get; set; }
        public string Description { get; set; }
        public int? CostCenterId { get; set; }
        public int? WarehouseId { get; set; }
    }
}

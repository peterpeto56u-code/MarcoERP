namespace MarcoERP.Application.DTOs.Accounting
{
    /// <summary>
    /// DTO for a single line in a new journal entry creation request.
    /// Validated via <see cref="Validators.Accounting.CreateJournalEntryDtoValidator"/>.
    /// </summary>
    public sealed class CreateJournalEntryLineDto
    {
        /// <summary>Account ID (must be postable).</summary>
        public int AccountId { get; set; }

        /// <summary>Debit amount (0 if credit). Non-negative.</summary>
        public decimal DebitAmount { get; set; }

        /// <summary>Credit amount (0 if debit). Non-negative.</summary>
        public decimal CreditAmount { get; set; }

        /// <summary>Line-level description (optional).</summary>
        public string Description { get; set; }

        /// <summary>Cost center (optional, overrides header).</summary>
        public int? CostCenterId { get; set; }

        /// <summary>Warehouse (optional, for inventory-related).</summary>
        public int? WarehouseId { get; set; }
    }
}

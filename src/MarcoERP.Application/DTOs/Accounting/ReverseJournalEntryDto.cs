namespace MarcoERP.Application.DTOs.Accounting
{
    /// <summary>
    /// DTO for requesting a journal entry reversal.
    /// </summary>
    public sealed class ReverseJournalEntryDto
    {
        /// <summary>ID of the posted journal entry to reverse.</summary>
        public int JournalEntryId { get; set; }

        /// <summary>Mandatory reason for the reversal.</summary>
        public string ReversalReason { get; set; }

        /// <summary>Date for the reversal entry.</summary>
        public System.DateTime ReversalDate { get; set; }
    }
}

namespace MarcoERP.Application.DTOs.Accounting
{
    /// <summary>
    /// Result DTO returned after a journal entry is successfully posted.
    /// </summary>
    public sealed class PostJournalResultDto
    {
        /// <summary>The journal entry ID.</summary>
        public int JournalEntryId { get; set; }

        /// <summary>The assigned final journal number (e.g., "JV-2026-00001").</summary>
        public string JournalNumber { get; set; }

        /// <summary>UTC timestamp of posting.</summary>
        public System.DateTime PostingDate { get; set; }
    }
}

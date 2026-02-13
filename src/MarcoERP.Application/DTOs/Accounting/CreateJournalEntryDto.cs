using System;
using System.Collections.Generic;
using MarcoERP.Domain.Enums;

namespace MarcoERP.Application.DTOs.Accounting
{
    /// <summary>
    /// DTO for creating a new journal entry (draft).
    /// All lines are provided upfront.
    /// Validated via <see cref="Validators.Accounting.CreateJournalEntryDtoValidator"/>.
    /// </summary>
    public sealed class CreateJournalEntryDto
    {
        /// <summary>Transaction date (must fall within open fiscal period).</summary>
        public DateTime JournalDate { get; set; }

        /// <summary>Narrative (mandatory per JE-INV-13).</summary>
        public string Description { get; set; }

        /// <summary>Source type (Manual for user-created entries).</summary>
        public SourceType SourceType { get; set; }

        /// <summary>External reference number (optional).</summary>
        public string ReferenceNumber { get; set; }

        /// <summary>Cost center for the whole entry (optional).</summary>
        public int? CostCenterId { get; set; }

        /// <summary>Journal entry lines (minimum 2).</summary>
        public List<CreateJournalEntryLineDto> Lines { get; set; } = new();
    }
}

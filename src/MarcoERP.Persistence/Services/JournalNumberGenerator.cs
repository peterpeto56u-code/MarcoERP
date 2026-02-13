using System.Linq;
using MarcoERP.Domain.Entities.Accounting;
using MarcoERP.Domain.Entities.Accounting.Policies;
using Microsoft.EntityFrameworkCore;

namespace MarcoERP.Persistence.Services
{
    /// <summary>
    /// Generates sequential journal numbers using the CodeSequence table.
    /// SEQ-03: Must run within Serializable isolation (ensured by the calling service).
    /// The caller (JournalEntryService) wraps the posting in a Serializable transaction.
    /// This generator reads/increments synchronously per IJournalNumberGenerator contract.
    /// </summary>
    public sealed class JournalNumberGenerator : IJournalNumberGenerator
    {
        private readonly MarcoDbContext _context;

        public JournalNumberGenerator(MarcoDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Returns the next sequential journal number for the given fiscal year.
        /// Format: JV-{Year}-{Sequence:D5} (e.g., JV-2026-00001).
        /// Creates the CodeSequence row if it doesn't exist.
        /// </summary>
        public string NextNumber(int fiscalYearId)
        {
            const string documentType = "JV";

            var sequence = _context.CodeSequences
                .FirstOrDefault(s => s.DocumentType == documentType && s.FiscalYearId == fiscalYearId);

            if (sequence == null)
            {
                var fiscalYear = _context.FiscalYears
                    .AsNoTracking()
                    .First(f => f.Id == fiscalYearId);

                var prefix = $"{documentType}-{fiscalYear.Year}-";
                sequence = new CodeSequence(documentType, fiscalYearId, prefix, 0);
                _context.CodeSequences.Add(sequence);
            }

            return sequence.NextCode();
        }
    }
}

using System.Linq;
using MarcoERP.Application.Interfaces;
using MarcoERP.Domain.Entities.Accounting;
using Microsoft.EntityFrameworkCore;

namespace MarcoERP.Persistence.Services
{
    /// <summary>
    /// Generates sequential document codes using the CodeSequence table.
    /// Supports all document types: JV, SI, PI, CR, CP, CT, SR, PR, IT.
    /// SEQ-03: Must run within Serializable isolation (ensured by the calling service).
    /// </summary>
    public sealed class CodeGenerator : ICodeGenerator
    {
        private readonly MarcoDbContext _context;

        public CodeGenerator(MarcoDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public string NextCode(string documentType, int fiscalYearId)
        {
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

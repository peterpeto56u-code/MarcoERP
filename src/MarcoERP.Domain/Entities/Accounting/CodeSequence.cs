namespace MarcoERP.Domain.Entities.Accounting
{
    /// <summary>
    /// Represents a persistent code sequence for auto-numbering documents.
    /// One row per document type per fiscal year.
    /// Managed at DB level with Serializable isolation (SEQ-03).
    /// </summary>
    public sealed class CodeSequence
    {
        /// <summary>EF Core only.</summary>
        private CodeSequence() { }

        public CodeSequence(string documentType, int fiscalYearId, string prefix, int currentSequence = 0)
        {
            DocumentType = documentType;
            FiscalYearId = fiscalYearId;
            Prefix = prefix;
            CurrentSequence = currentSequence;
        }

        /// <summary>Primary key.</summary>
        public int Id { get; private set; }

        /// <summary>Document type identifier (e.g., "JV", "SI", "PI").</summary>
        public string DocumentType { get; private set; }

        /// <summary>FK to fiscal year — sequences reset per year.</summary>
        public int FiscalYearId { get; private set; }

        /// <summary>Prefix for the generated code (e.g., "JV-2026-").</summary>
        public string Prefix { get; private set; }

        /// <summary>Current highest sequence number consumed.</summary>
        public int CurrentSequence { get; private set; }

        /// <summary>Concurrency token.</summary>
        public byte[] RowVersion { get; set; }

        /// <summary>
        /// Increments the sequence and returns the next formatted code.
        /// Called within Serializable transaction.
        /// </summary>
        public string NextCode()
        {
            CurrentSequence++;
            return $"{Prefix}{CurrentSequence:D5}";
        }
    }
}

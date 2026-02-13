namespace MarcoERP.Application.DTOs.Treasury
{
    // ════════════════════════════════════════════════════════════
    //  Bank Account DTOs
    // ════════════════════════════════════════════════════════════

    /// <summary>Full bank account details.</summary>
    public sealed class BankAccountDto
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string BankName { get; set; }
        public string AccountNumber { get; set; }
        public string IBAN { get; set; }
        public int? AccountId { get; set; }
        public string AccountName { get; set; }
        public bool IsActive { get; set; }
        public bool IsDefault { get; set; }
    }

    /// <summary>DTO for creating a new bank account.</summary>
    public sealed class CreateBankAccountDto
    {
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string BankName { get; set; }
        public string AccountNumber { get; set; }
        public string IBAN { get; set; }
        public int? AccountId { get; set; }
    }

    /// <summary>DTO for updating an existing bank account.</summary>
    public sealed class UpdateBankAccountDto
    {
        public int Id { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string BankName { get; set; }
        public string AccountNumber { get; set; }
        public string IBAN { get; set; }
        public int? AccountId { get; set; }
    }
}

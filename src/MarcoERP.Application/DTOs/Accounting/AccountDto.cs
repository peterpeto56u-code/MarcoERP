using MarcoERP.Domain.Enums;

namespace MarcoERP.Application.DTOs.Accounting
{
    /// <summary>
    /// Read-only DTO representing an account in the Chart of Accounts.
    /// Used for display, listing, and lookups.
    /// </summary>
    public sealed class AccountDto
    {
        public int Id { get; set; }
        public string AccountCode { get; set; }
        public string AccountNameAr { get; set; }
        public string AccountNameEn { get; set; }
        public AccountType AccountType { get; set; }
        public NormalBalance NormalBalance { get; set; }
        public int? ParentAccountId { get; set; }
        public string ParentAccountCode { get; set; }
        public string ParentAccountNameAr { get; set; }
        public int Level { get; set; }
        public bool IsLeaf { get; set; }
        public bool AllowPosting { get; set; }
        public bool IsActive { get; set; }
        public bool IsSystemAccount { get; set; }
        public string CurrencyCode { get; set; }
        public string Description { get; set; }
        public bool HasPostings { get; set; }
        public byte[] RowVersion { get; set; }
    }
}

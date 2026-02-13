using MarcoERP.Domain.Enums;

namespace MarcoERP.Application.DTOs.Accounting
{
    /// <summary>
    /// DTO for creating a new account.
    /// Validated via <see cref="Validators.Accounting.CreateAccountDtoValidator"/>.
    /// </summary>
    public sealed class CreateAccountDto
    {
        /// <summary>4-digit numeric account code (e.g., "1111").</summary>
        public string AccountCode { get; set; }

        /// <summary>Arabic account name (required).</summary>
        public string AccountNameAr { get; set; }

        /// <summary>English account name (optional).</summary>
        public string AccountNameEn { get; set; }

        /// <summary>Account classification.</summary>
        public AccountType AccountType { get; set; }

        /// <summary>Parent account ID (null for Level 1 root accounts).</summary>
        public int? ParentAccountId { get; set; }

        /// <summary>Hierarchy level 1–4.</summary>
        public int Level { get; set; }

        /// <summary>Whether this is a system-protected account.</summary>
        public bool IsSystemAccount { get; set; }

        /// <summary>ISO 4217 currency code (e.g., "EGP").</summary>
        public string CurrencyCode { get; set; }

        /// <summary>Optional description.</summary>
        public string Description { get; set; }
    }
}

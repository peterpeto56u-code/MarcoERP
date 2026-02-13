namespace MarcoERP.Application.DTOs.Accounting
{
    /// <summary>
    /// DTO for updating an existing account.
    /// Only mutable fields are exposed. AccountCode, Level, AccountType (if posted) are immutable.
    /// </summary>
    public sealed class UpdateAccountDto
    {
        /// <summary>Account ID to update.</summary>
        public int Id { get; set; }

        /// <summary>Updated Arabic name.</summary>
        public string AccountNameAr { get; set; }

        /// <summary>Updated English name.</summary>
        public string AccountNameEn { get; set; }

        /// <summary>Updated description.</summary>
        public string Description { get; set; }

        /// <summary>Activate or deactivate.</summary>
        public bool IsActive { get; set; }

        /// <summary>Optimistic concurrency token (original from load).</summary>
        public byte[] RowVersion { get; set; }
    }
}

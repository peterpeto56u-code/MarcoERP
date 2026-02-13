using MarcoERP.Domain.Exceptions;

namespace MarcoERP.Domain.Entities.Security
{
    /// <summary>
    /// Represents a permission granted to a role.
    /// Permission keys are dot-separated strings: "Module.Action"
    /// Examples: Accounting.PostJournal, Sales.PostInvoice, Treasury.Transfer
    /// </summary>
    public sealed class RolePermission
    {
        // ── Constructors ────────────────────────────────────────

        /// <summary>EF Core only.</summary>
        private RolePermission() { }

        /// <summary>
        /// Creates a new permission entry with the given key.
        /// </summary>
        internal RolePermission(string permissionKey)
        {
            if (string.IsNullOrWhiteSpace(permissionKey))
                throw new SecurityDomainException("مفتاح الصلاحية مطلوب.");
            PermissionKey = permissionKey.Trim();
        }

        // ── Properties ──────────────────────────────────────────

        /// <summary>Primary key.</summary>
        public int Id { get; private set; }

        /// <summary>Foreign key to the owning role.</summary>
        public int RoleId { get; private set; }

        /// <summary>Navigation to the owning role.</summary>
        public Role Role { get; private set; }

        /// <summary>
        /// The permission key string (e.g. "Accounting.PostJournal").
        /// Convention: {Module}.{Action}
        /// </summary>
        public string PermissionKey { get; private set; }
    }
}

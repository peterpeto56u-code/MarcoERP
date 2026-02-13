using System.Collections.Generic;
using MarcoERP.Domain.Entities.Common;
using MarcoERP.Domain.Exceptions;

namespace MarcoERP.Domain.Entities.Security
{
    /// <summary>
    /// Represents a security role that groups permissions.
    /// Roles are system-seeded and managed by administrators.
    /// Per v1.1: Administrator, Accountant, Sales User, Storekeeper, Viewer.
    /// </summary>
    public sealed class Role : BaseEntity
    {
        private readonly List<RolePermission> _permissions = new();
        private readonly List<User> _users = new();

        // ── Constructors ────────────────────────────────────────

        /// <summary>EF Core only.</summary>
        private Role() { }

        /// <summary>
        /// Creates a new role with the specified names.
        /// </summary>
        public Role(string nameAr, string nameEn, string description, bool isSystem = false)
        {
            if (string.IsNullOrWhiteSpace(nameAr))
                throw new SecurityDomainException("اسم الدور بالعربية مطلوب.");
            if (string.IsNullOrWhiteSpace(nameEn))
                throw new SecurityDomainException("اسم الدور بالإنجليزية مطلوب.");

            NameAr = nameAr.Trim();
            NameEn = nameEn.Trim();
            Description = description?.Trim();
            IsSystem = isSystem;
        }

        // ── Properties ──────────────────────────────────────────

        /// <summary>Role name in Arabic.</summary>
        public string NameAr { get; private set; }

        /// <summary>Role name in English.</summary>
        public string NameEn { get; private set; }

        /// <summary>Optional description of the role.</summary>
        public string Description { get; private set; }

        /// <summary>
        /// If true, this role is system-defined and cannot be deleted.
        /// The 5 default roles (Administrator, Accountant, etc.) are system roles.
        /// </summary>
        public bool IsSystem { get; private set; }

        /// <summary>Navigation to the role's permissions.</summary>
        public IReadOnlyCollection<RolePermission> Permissions => _permissions.AsReadOnly();

        /// <summary>Navigation to users assigned to this role.</summary>
        public IReadOnlyCollection<User> Users => _users.AsReadOnly();

        // ── Domain Methods ──────────────────────────────────────

        /// <summary>
        /// Updates the role's display information.
        /// </summary>
        public void Update(string nameAr, string nameEn, string description)
        {
            if (string.IsNullOrWhiteSpace(nameAr))
                throw new SecurityDomainException("اسم الدور بالعربية مطلوب.");
            if (string.IsNullOrWhiteSpace(nameEn))
                throw new SecurityDomainException("اسم الدور بالإنجليزية مطلوب.");

            NameAr = nameAr.Trim();
            NameEn = nameEn.Trim();
            Description = description?.Trim();
        }

        /// <summary>
        /// Adds a permission to this role.
        /// </summary>
        public void AddPermission(string permissionKey)
        {
            if (string.IsNullOrWhiteSpace(permissionKey))
                throw new SecurityDomainException("مفتاح الصلاحية مطلوب.");

            if (_permissions.Exists(p => p.PermissionKey == permissionKey))
                return; // Idempotent

            _permissions.Add(new RolePermission(permissionKey));
        }

        /// <summary>
        /// Removes a permission from this role.
        /// </summary>
        public void RemovePermission(string permissionKey)
        {
            _permissions.RemoveAll(p => p.PermissionKey == permissionKey);
        }

        /// <summary>
        /// Checks if this role has a specific permission.
        /// </summary>
        public bool HasPermission(string permissionKey)
        {
            return _permissions.Exists(p => p.PermissionKey == permissionKey);
        }

        /// <summary>
        /// Replaces all permissions with the given set.
        /// </summary>
        public void SetPermissions(IEnumerable<string> permissionKeys)
        {
            _permissions.Clear();
            foreach (var key in permissionKeys)
            {
                if (!string.IsNullOrWhiteSpace(key))
                    _permissions.Add(new RolePermission(key));
            }
        }
    }
}

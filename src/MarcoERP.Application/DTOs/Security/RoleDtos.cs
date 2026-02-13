using System.Collections.Generic;

namespace MarcoERP.Application.DTOs.Security
{
    // ════════════════════════════════════════════════════════════
    //  Role DTOs (إدارة الأدوار)
    // ════════════════════════════════════════════════════════════

    /// <summary>Full role details with permissions.</summary>
    public sealed class RoleDto
    {
        public int Id { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string Description { get; set; }
        public bool IsSystem { get; set; }
        public List<string> Permissions { get; set; } = new();
    }

    /// <summary>Lightweight role for dropdowns / lists.</summary>
    public sealed class RoleListDto
    {
        public int Id { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public bool IsSystem { get; set; }
        public int UserCount { get; set; }
    }

    /// <summary>DTO for creating a new role.</summary>
    public sealed class CreateRoleDto
    {
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string Description { get; set; }
        public List<string> Permissions { get; set; } = new();
    }

    /// <summary>DTO for updating an existing role.</summary>
    public sealed class UpdateRoleDto
    {
        public int Id { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string Description { get; set; }
        public List<string> Permissions { get; set; } = new();
    }
}

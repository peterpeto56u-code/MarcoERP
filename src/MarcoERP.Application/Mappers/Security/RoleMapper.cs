using System.Linq;
using MarcoERP.Application.DTOs.Security;
using MarcoERP.Domain.Entities.Security;

namespace MarcoERP.Application.Mappers.Security
{
    /// <summary>Manual mapper for Role entity ↔ DTOs.</summary>
    public static class RoleMapper
    {
        public static RoleDto ToDto(Role entity)
        {
            if (entity == null) return null;

            return new RoleDto
            {
                Id = entity.Id,
                NameAr = entity.NameAr,
                NameEn = entity.NameEn,
                Description = entity.Description,
                IsSystem = entity.IsSystem,
                Permissions = entity.Permissions?.Select(p => p.PermissionKey).ToList() ?? new()
            };
        }

        public static RoleListDto ToListDto(Role entity)
        {
            if (entity == null) return null;

            return new RoleListDto
            {
                Id = entity.Id,
                NameAr = entity.NameAr,
                NameEn = entity.NameEn,
                IsSystem = entity.IsSystem,
                UserCount = entity.Users?.Count ?? 0
            };
        }
    }
}

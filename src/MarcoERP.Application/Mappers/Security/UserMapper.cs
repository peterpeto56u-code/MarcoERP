using System.Linq;
using MarcoERP.Application.DTOs.Security;
using MarcoERP.Domain.Entities.Security;

namespace MarcoERP.Application.Mappers.Security
{
    /// <summary>Manual mapper for User entity ↔ DTOs.</summary>
    public static class UserMapper
    {
        public static UserDto ToDto(User entity)
        {
            if (entity == null) return null;

            return new UserDto
            {
                Id = entity.Id,
                Username = entity.Username,
                FullNameAr = entity.FullNameAr,
                FullNameEn = entity.FullNameEn,
                Email = entity.Email,
                Phone = entity.Phone,
                RoleId = entity.RoleId,
                RoleNameAr = entity.Role?.NameAr,
                RoleNameEn = entity.Role?.NameEn,
                IsActive = entity.IsActive,
                IsLocked = entity.IsLocked,
                FailedLoginAttempts = entity.FailedLoginAttempts,
                LastLoginAt = entity.LastLoginAt,
                MustChangePassword = entity.MustChangePassword,
                CreatedAt = entity.CreatedAt,
                CreatedBy = entity.CreatedBy
            };
        }

        public static UserListDto ToListDto(User entity)
        {
            if (entity == null) return null;

            return new UserListDto
            {
                Id = entity.Id,
                Username = entity.Username,
                FullNameAr = entity.FullNameAr,
                RoleNameAr = entity.Role?.NameAr,
                IsActive = entity.IsActive,
                IsLocked = entity.IsLocked,
                LastLoginAt = entity.LastLoginAt
            };
        }
    }
}

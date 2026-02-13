using MarcoERP.Application.DTOs.Settings;
using MarcoERP.Domain.Entities.Settings;

namespace MarcoERP.Application.Mappers.Settings
{
    /// <summary>
    /// Maps between SystemProfile entity and SystemProfileDto.
    /// Phase 3: Progressive Complexity Layer.
    /// </summary>
    public static class ProfileMapper
    {
        public static SystemProfileDto ToDto(SystemProfile entity)
        {
            if (entity == null) return null;
            return new SystemProfileDto
            {
                Id = entity.Id,
                ProfileName = entity.ProfileName,
                Description = entity.Description,
                IsActive = entity.IsActive
            };
        }
    }
}

using MarcoERP.Application.DTOs.Settings;
using MarcoERP.Domain.Entities.Settings;

namespace MarcoERP.Application.Mappers.Settings
{
    /// <summary>Manual mapper for SystemSetting entity ↔ DTOs.</summary>
    public static class SystemSettingMapper
    {
        public static SystemSettingDto ToDto(SystemSetting entity)
        {
            if (entity == null) return null;

            return new SystemSettingDto
            {
                Id = entity.Id,
                SettingKey = entity.SettingKey,
                SettingValue = entity.SettingValue,
                Description = entity.Description,
                GroupName = entity.GroupName,
                DataType = entity.DataType
            };
        }
    }
}

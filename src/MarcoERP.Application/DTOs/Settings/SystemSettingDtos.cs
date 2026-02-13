using System.Collections.Generic;

namespace MarcoERP.Application.DTOs.Settings
{
    // ════════════════════════════════════════════════════════════
    //  SystemSetting DTOs (إعدادات النظام)
    // ════════════════════════════════════════════════════════════

    /// <summary>Full setting details for display/edit.</summary>
    public sealed class SystemSettingDto
    {
        public int Id { get; set; }
        public string SettingKey { get; set; }
        public string SettingValue { get; set; }
        public string Description { get; set; }
        public string GroupName { get; set; }
        public string DataType { get; set; }
    }

    /// <summary>DTO for updating a setting value.</summary>
    public sealed class UpdateSystemSettingDto
    {
        public string SettingKey { get; set; }
        public string SettingValue { get; set; }
    }

    /// <summary>A group of settings for sectioned display.</summary>
    public sealed class SettingGroupDto
    {
        public string GroupName { get; set; }
        public List<SystemSettingDto> Settings { get; set; } = new();
    }
}

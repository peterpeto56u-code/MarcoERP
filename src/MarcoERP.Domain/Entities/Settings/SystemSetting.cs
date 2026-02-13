using MarcoERP.Domain.Entities.Common;

namespace MarcoERP.Domain.Entities.Settings
{
    /// <summary>
    /// Key-value system configuration setting.
    /// Used for application-wide settings like default accounts, company info, precision, etc.
    /// Per v1.1 Phase 5D: removes hardcoded system accounts and increases flexibility.
    /// </summary>
    public sealed class SystemSetting : BaseEntity
    {
        // ── Constructors ────────────────────────────────────────

        /// <summary>EF Core only.</summary>
        private SystemSetting() { }

        /// <summary>
        /// Creates a new system setting.
        /// </summary>
        public SystemSetting(string settingKey, string settingValue, string description, string groupName, string dataType)
        {
            if (string.IsNullOrWhiteSpace(settingKey))
                throw new System.ArgumentException("مفتاح الإعداد مطلوب.", nameof(settingKey));

            SettingKey = settingKey.Trim();
            SettingValue = settingValue?.Trim();
            Description = description?.Trim();
            GroupName = groupName?.Trim();
            DataType = dataType?.Trim() ?? "string";
        }

        // ── Properties ──────────────────────────────────────────

        /// <summary>Unique setting key (e.g. "DefaultCashboxId", "CompanyName").</summary>
        public string SettingKey { get; private set; }

        /// <summary>Setting value stored as string. Cast based on DataType.</summary>
        public string SettingValue { get; private set; }

        /// <summary>Arabic description of the setting for display.</summary>
        public string Description { get; private set; }

        /// <summary>Group name for UI display (e.g. "حسابات افتراضية", "معلومات الشركة").</summary>
        public string GroupName { get; private set; }

        /// <summary>Data type hint: string, int, decimal, bool.</summary>
        public string DataType { get; private set; }

        // ── Domain Methods ──────────────────────────────────────

        /// <summary>
        /// Updates the setting value.
        /// </summary>
        public void UpdateValue(string newValue)
        {
            SettingValue = newValue?.Trim();
        }
    }
}

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MarcoERP.Domain.Entities.Settings;

namespace MarcoERP.Domain.Interfaces.Settings
{
    /// <summary>
    /// Repository contract for SystemSetting entity.
    /// </summary>
    public interface ISystemSettingRepository : IRepository<SystemSetting>
    {
        /// <summary>Gets a setting by its unique key.</summary>
        Task<SystemSetting> GetByKeyAsync(string settingKey, CancellationToken ct = default);

        /// <summary>Gets all settings grouped by GroupName.</summary>
        Task<IReadOnlyList<SystemSetting>> GetByGroupAsync(string groupName, CancellationToken ct = default);

        /// <summary>Checks if a setting key exists.</summary>
        Task<bool> KeyExistsAsync(string settingKey, CancellationToken ct = default);
    }
}

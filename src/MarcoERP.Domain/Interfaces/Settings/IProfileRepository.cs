using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MarcoERP.Domain.Entities.Settings;

namespace MarcoERP.Domain.Interfaces.Settings
{
    /// <summary>
    /// Repository contract for SystemProfile and ProfileFeature entities.
    /// Phase 3: Progressive Complexity Layer.
    /// </summary>
    public interface IProfileRepository : IRepository<SystemProfile>
    {
        /// <summary>Gets a profile by name.</summary>
        Task<SystemProfile> GetByNameAsync(string profileName, CancellationToken ct = default);

        /// <summary>Gets the currently active profile.</summary>
        Task<SystemProfile> GetActiveProfileAsync(CancellationToken ct = default);

        /// <summary>Gets all feature keys mapped to a specific profile.</summary>
        Task<IReadOnlyList<string>> GetFeatureKeysForProfileAsync(int profileId, CancellationToken ct = default);

        /// <summary>Gets all feature keys mapped to a profile by name.</summary>
        Task<IReadOnlyList<string>> GetFeatureKeysForProfileAsync(string profileName, CancellationToken ct = default);
    }
}

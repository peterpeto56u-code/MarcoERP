using System.Threading;
using System.Threading.Tasks;
using MarcoERP.Domain.Entities.Settings;

namespace MarcoERP.Application.Interfaces.Settings
{
    /// <summary>
    /// Repository contract for SystemVersion and FeatureVersion entities.
    /// Phase 5: Version &amp; Integrity Engine.
    /// </summary>
    public interface IVersionRepository
    {
        /// <summary>Gets the latest registered system version.</summary>
        Task<SystemVersion> GetLatestVersionAsync(CancellationToken ct = default);

        /// <summary>Checks if a version number is already registered.</summary>
        Task<bool> VersionExistsAsync(string versionNumber, CancellationToken ct = default);

        /// <summary>Adds a new system version entry.</summary>
        Task AddAsync(SystemVersion version, CancellationToken ct = default);

        /// <summary>Gets the FeatureVersion mapping for a feature key.</summary>
        Task<FeatureVersion> GetFeatureVersionAsync(string featureKey, CancellationToken ct = default);
    }
}

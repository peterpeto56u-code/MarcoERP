using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MarcoERP.Domain.Entities.Settings;

namespace MarcoERP.Domain.Interfaces.Settings
{
    /// <summary>
    /// Repository contract for Feature entity.
    /// Phase 2: Feature Governance Engine.
    /// </summary>
    public interface IFeatureRepository : IRepository<Feature>
    {
        /// <summary>Gets a feature by its unique key.</summary>
        Task<Feature> GetByKeyAsync(string featureKey, CancellationToken ct = default);

        /// <summary>Checks if a feature key exists.</summary>
        Task<bool> KeyExistsAsync(string featureKey, CancellationToken ct = default);

        /// <summary>Adds a feature change log entry.</summary>
        Task AddChangeLogAsync(FeatureChangeLog log, CancellationToken ct = default);

        /// <summary>Gets change log entries for a specific feature.</summary>
        Task<IReadOnlyList<FeatureChangeLog>> GetChangeLogsAsync(int featureId, CancellationToken ct = default);
    }
}

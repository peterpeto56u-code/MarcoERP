using System.Threading;
using System.Threading.Tasks;
using MarcoERP.Application.DTOs.Settings;

namespace MarcoERP.Application.Interfaces.Settings
{
    /// <summary>
    /// Analyzes the impact of enabling/disabling a feature before execution.
    /// Phase 4: Impact Analyzer — read-only analysis, no behavioral changes.
    /// </summary>
    public interface IImpactAnalyzerService
    {
        /// <summary>
        /// Produces a full impact report for the specified feature key.
        /// Does NOT execute any changes.
        /// </summary>
        Task<FeatureImpactReport> AnalyzeAsync(string featureKey, CancellationToken ct = default);
    }
}

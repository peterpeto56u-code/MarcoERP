using System.Threading;
using System.Threading.Tasks;
using MarcoERP.Application.Interfaces.Settings;

namespace MarcoERP.Application.Common
{
    /// <summary>
    /// Static helper for checking feature availability before executing operations.
    /// Phase 2: Feature Governance Engine — NOT wired into any existing service yet.
    /// Future phases will inject this check at the Application layer entry points.
    /// </summary>
    public static class FeatureGuard
    {
        private const string FeatureDisabledMsg = "هذه الميزة معطلة حاليًا: {0}";

        /// <summary>
        /// Checks if a feature is enabled. Returns a failed ServiceResult if disabled; null if enabled.
        /// Usage: var guard = await FeatureGuard.CheckAsync(featureService, "AdvancedAccounting", ct);
        ///        if (guard != null) return guard;
        /// </summary>
        public static async Task<ServiceResult> CheckAsync(
            IFeatureService featureService,
            string featureKey,
            CancellationToken ct = default)
        {
            var result = await featureService.IsEnabledAsync(featureKey, ct);

            // If feature not found, assume enabled (safe default)
            if (result.IsFailure)
                return null;

            if (!result.Data)
                return ServiceResult.Failure(string.Format(FeatureDisabledMsg, featureKey));

            return null; // enabled — caller proceeds
        }

        /// <summary>
        /// Generic version for services returning ServiceResult{T}.
        /// </summary>
        public static async Task<ServiceResult<T>> CheckAsync<T>(
            IFeatureService featureService,
            string featureKey,
            CancellationToken ct = default)
        {
            var result = await featureService.IsEnabledAsync(featureKey, ct);

            // If feature not found, assume enabled (safe default)
            if (result.IsFailure)
                return null;

            if (!result.Data)
                return ServiceResult<T>.Failure(string.Format(FeatureDisabledMsg, featureKey));

            return null; // enabled — caller proceeds
        }
    }
}

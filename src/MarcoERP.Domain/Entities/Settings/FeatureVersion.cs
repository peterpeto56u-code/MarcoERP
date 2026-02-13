using System;
using MarcoERP.Domain.Entities.Common;

namespace MarcoERP.Domain.Entities.Settings
{
    /// <summary>
    /// Maps each Feature to the version in which it was introduced.
    /// Phase 5: Version &amp; Integrity Engine.
    /// </summary>
    public sealed class FeatureVersion : BaseEntity
    {
        /// <summary>EF Core only.</summary>
        private FeatureVersion() { }

        /// <summary>
        /// Creates a feature-version mapping.
        /// </summary>
        public FeatureVersion(string featureKey, string introducedInVersion)
        {
            if (string.IsNullOrWhiteSpace(featureKey))
                throw new ArgumentException("مفتاح الميزة مطلوب.", nameof(featureKey));
            if (string.IsNullOrWhiteSpace(introducedInVersion))
                throw new ArgumentException("رقم الإصدار مطلوب.", nameof(introducedInVersion));

            FeatureKey = featureKey.Trim();
            IntroducedInVersion = introducedInVersion.Trim();
        }

        /// <summary>The feature key (matches Feature.FeatureKey).</summary>
        public string FeatureKey { get; private set; }

        /// <summary>The version in which this feature was first introduced.</summary>
        public string IntroducedInVersion { get; private set; }
    }
}

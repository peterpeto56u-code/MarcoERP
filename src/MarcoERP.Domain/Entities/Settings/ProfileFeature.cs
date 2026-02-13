using MarcoERP.Domain.Entities.Common;

namespace MarcoERP.Domain.Entities.Settings
{
    /// <summary>
    /// Maps which features are included in each profile.
    /// Phase 3: Progressive Complexity Layer.
    /// </summary>
    public sealed class ProfileFeature : BaseEntity
    {
        // ── Constructors ────────────────────────────────────────

        /// <summary>EF Core only.</summary>
        private ProfileFeature() { }

        /// <summary>
        /// Creates a new profile-feature mapping.
        /// </summary>
        public ProfileFeature(int profileId, string featureKey)
        {
            if (string.IsNullOrWhiteSpace(featureKey))
                throw new System.ArgumentException("مفتاح الميزة مطلوب.", nameof(featureKey));

            ProfileId = profileId;
            FeatureKey = featureKey.Trim();
        }

        // ── Properties ──────────────────────────────────────────

        /// <summary>FK to the SystemProfile.</summary>
        public int ProfileId { get; private set; }

        /// <summary>Feature key that belongs to this profile.</summary>
        public string FeatureKey { get; private set; }

        // ── Navigation ──────────────────────────────────────────

        /// <summary>Navigation property to the SystemProfile.</summary>
        public SystemProfile Profile { get; private set; }
    }
}

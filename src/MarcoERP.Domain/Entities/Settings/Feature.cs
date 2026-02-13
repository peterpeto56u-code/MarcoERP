using MarcoERP.Domain.Entities.Common;

namespace MarcoERP.Domain.Entities.Settings
{
    /// <summary>
    /// Represents a toggleable system feature (module / capability).
    /// Phase 2: Feature Governance Engine — safe mode, no behavioral changes.
    /// </summary>
    public sealed class Feature : AuditableEntity
    {
        // ── Constructors ────────────────────────────────────────

        /// <summary>EF Core only.</summary>
        private Feature() { }

        /// <summary>
        /// Creates a new feature definition.
        /// </summary>
        public Feature(string featureKey, string nameAr, string nameEn, string description, bool isEnabled, string riskLevel, string dependsOn = null)
        {
            if (string.IsNullOrWhiteSpace(featureKey))
                throw new System.ArgumentException("مفتاح الميزة مطلوب.", nameof(featureKey));
            if (string.IsNullOrWhiteSpace(nameAr))
                throw new System.ArgumentException("اسم الميزة بالعربية مطلوب.", nameof(nameAr));

            FeatureKey = featureKey.Trim();
            NameAr = nameAr.Trim();
            NameEn = nameEn?.Trim();
            Description = description?.Trim();
            IsEnabled = isEnabled;
            RiskLevel = riskLevel?.Trim() ?? DomainConstants.DefaultPriority;
            DependsOn = dependsOn?.Trim();
        }

        // ── Properties ──────────────────────────────────────────

        /// <summary>Unique key identifying the feature (e.g. "AdvancedAccounting").</summary>
        public string FeatureKey { get; private set; }

        /// <summary>Arabic display name.</summary>
        public string NameAr { get; private set; }

        /// <summary>English display name.</summary>
        public string NameEn { get; private set; }

        /// <summary>Description of the feature purpose.</summary>
        public string Description { get; private set; }

        /// <summary>Whether the feature is currently enabled.</summary>
        public bool IsEnabled { get; private set; }

        /// <summary>Risk level: Low, Medium, High.</summary>
        public string RiskLevel { get; private set; }

        /// <summary>Comma-separated list of FeatureKeys this feature depends on (nullable).</summary>
        public string DependsOn { get; private set; }

        // ── Impact Analysis Properties (Phase 4) ────────────────

        /// <summary>Whether enabling/disabling this feature affects stored data.</summary>
        public bool AffectsData { get; private set; }

        /// <summary>Whether this feature requires a database migration to function.</summary>
        public bool RequiresMigration { get; private set; }

        /// <summary>Whether this feature affects the accounting sub-system.</summary>
        public bool AffectsAccounting { get; private set; }

        /// <summary>Whether this feature affects the inventory sub-system.</summary>
        public bool AffectsInventory { get; private set; }

        /// <summary>Whether this feature affects the reporting sub-system.</summary>
        public bool AffectsReporting { get; private set; }

        /// <summary>Free-text description of the impact when toggling this feature.</summary>
        public string ImpactDescription { get; private set; }

        // ── Domain Methods ──────────────────────────────────────

        /// <summary>
        /// Enables this feature.
        /// </summary>
        public void Enable()
        {
            IsEnabled = true;
        }

        /// <summary>
        /// Disables this feature.
        /// </summary>
        public void Disable()
        {
            IsEnabled = false;
        }

        /// <summary>
        /// Sets impact analysis metadata for this feature.
        /// Phase 4: Impact Analyzer.
        /// </summary>
        public void SetImpactMetadata(
            bool affectsData,
            bool requiresMigration,
            bool affectsAccounting,
            bool affectsInventory,
            bool affectsReporting,
            string impactDescription)
        {
            AffectsData = affectsData;
            RequiresMigration = requiresMigration;
            AffectsAccounting = affectsAccounting;
            AffectsInventory = affectsInventory;
            AffectsReporting = affectsReporting;
            ImpactDescription = impactDescription?.Trim();
        }
    }
}

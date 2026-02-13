namespace MarcoERP.Application.DTOs.Settings
{
    /// <summary>
    /// Read-only DTO for Feature entity.
    /// Phase 2: Feature Governance Engine.
    /// </summary>
    public sealed class FeatureDto
    {
        public int Id { get; set; }
        public string FeatureKey { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string Description { get; set; }
        public bool IsEnabled { get; set; }
        public string RiskLevel { get; set; }
        public string DependsOn { get; set; }

        // Phase 4: Impact fields
        public bool AffectsData { get; set; }
        public bool RequiresMigration { get; set; }
        public bool AffectsAccounting { get; set; }
        public bool AffectsInventory { get; set; }
        public bool AffectsReporting { get; set; }
        public string ImpactDescription { get; set; }
    }
}

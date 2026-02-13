using System.Collections.Generic;

namespace MarcoERP.Application.DTOs.Settings
{
    /// <summary>
    /// Impact analysis report for a feature toggle operation.
    /// Phase 4: Impact Analyzer — read-only analysis, no behavioral changes.
    /// </summary>
    public sealed class FeatureImpactReport
    {
        public string FeatureKey { get; set; }
        public string RiskLevel { get; set; }
        public bool RequiresMigration { get; set; }
        public List<string> Dependencies { get; set; } = new();
        public List<string> ImpactAreas { get; set; } = new();
        public List<string> DisabledDependencies { get; set; } = new();
        public string WarningMessage { get; set; }
        public bool CanProceed { get; set; } = true;
    }
}

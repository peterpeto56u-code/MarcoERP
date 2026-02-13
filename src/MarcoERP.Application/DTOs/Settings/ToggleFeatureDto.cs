namespace MarcoERP.Application.DTOs.Settings
{
    /// <summary>
    /// DTO for toggling a feature on/off.
    /// Phase 2: Feature Governance Engine.
    /// </summary>
    public sealed class ToggleFeatureDto
    {
        public string FeatureKey { get; set; }
        public bool IsEnabled { get; set; }
    }
}

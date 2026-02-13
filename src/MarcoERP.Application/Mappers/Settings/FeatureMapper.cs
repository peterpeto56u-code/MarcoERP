using MarcoERP.Application.DTOs.Settings;
using MarcoERP.Domain.Entities.Settings;

namespace MarcoERP.Application.Mappers.Settings
{
    /// <summary>
    /// Maps between Feature entity and FeatureDto.
    /// Phase 2: Feature Governance Engine.
    /// </summary>
    public static class FeatureMapper
    {
        public static FeatureDto ToDto(Feature entity)
        {
            if (entity == null) return null;
            return new FeatureDto
            {
                Id = entity.Id,
                FeatureKey = entity.FeatureKey,
                NameAr = entity.NameAr,
                NameEn = entity.NameEn,
                Description = entity.Description,
                IsEnabled = entity.IsEnabled,
                RiskLevel = entity.RiskLevel,
                DependsOn = entity.DependsOn,
                AffectsData = entity.AffectsData,
                RequiresMigration = entity.RequiresMigration,
                AffectsAccounting = entity.AffectsAccounting,
                AffectsInventory = entity.AffectsInventory,
                AffectsReporting = entity.AffectsReporting,
                ImpactDescription = entity.ImpactDescription
            };
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MarcoERP.Application.Common;
using MarcoERP.Application.DTOs.Settings;
using MarcoERP.Application.Interfaces.Settings;
using MarcoERP.Domain.Enums;
using MarcoERP.Domain.Interfaces.Settings;

namespace MarcoERP.Application.Services.Settings
{
    /// <summary>
    /// Analyzes the impact of enabling/disabling a feature.
    /// Phase 4: Impact Analyzer — read-only analysis, no side effects.
    /// </summary>
    [Module(SystemModule.Settings)]
    public sealed class ImpactAnalyzerService : IImpactAnalyzerService
    {
        private readonly IFeatureRepository _featureRepo;

        public ImpactAnalyzerService(IFeatureRepository featureRepo)
        {
            _featureRepo = featureRepo ?? throw new ArgumentNullException(nameof(featureRepo));
        }

        public async Task<FeatureImpactReport> AnalyzeAsync(string featureKey, CancellationToken ct = default)
        {
            var report = new FeatureImpactReport { FeatureKey = featureKey };

            // 1. Fetch the feature
            var feature = await _featureRepo.GetByKeyAsync(featureKey, ct);
            if (feature == null)
            {
                report.RiskLevel = "Unknown";
                report.WarningMessage = $"الميزة '{featureKey}' غير موجودة في النظام.";
                report.CanProceed = false;
                return report;
            }

            // 2. Risk level
            report.RiskLevel = feature.RiskLevel ?? "Medium";

            // 3. Migration requirement
            report.RequiresMigration = feature.RequiresMigration;

            // 4. Parse dependencies
            if (!string.IsNullOrWhiteSpace(feature.DependsOn))
            {
                report.Dependencies = feature.DependsOn
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .ToList();
            }

            // 5. Build impact areas
            if (feature.AffectsAccounting) report.ImpactAreas.Add("المحاسبة (Accounting)");
            if (feature.AffectsInventory)  report.ImpactAreas.Add("المخزون (Inventory)");
            if (feature.AffectsReporting)  report.ImpactAreas.Add("التقارير (Reporting)");
            if (feature.AffectsData)       report.ImpactAreas.Add("البيانات المخزنة (Stored Data)");

            // 6. Check disabled dependencies (only relevant when enabling)
            if (!feature.IsEnabled && report.Dependencies.Count > 0)
            {
                var allFeatures = await _featureRepo.GetAllAsync(ct);
                var featureMap = allFeatures.ToDictionary(f => f.FeatureKey, f => f.IsEnabled);

                foreach (var dep in report.Dependencies)
                {
                    if (featureMap.TryGetValue(dep, out bool isEnabled) && !isEnabled)
                    {
                        report.DisabledDependencies.Add(dep);
                    }
                }

                if (report.DisabledDependencies.Count > 0)
                {
                    report.CanProceed = false;
                }
            }

            // 7. Build dynamic warning message
            report.WarningMessage = BuildWarningMessage(feature, report);

            return report;
        }

        // ── Private Helpers ──────────────────────────────────────

        private static string BuildWarningMessage(
            Domain.Entities.Settings.Feature feature,
            FeatureImpactReport report)
        {
            var warnings = new List<string>();

            // Risk warning
            switch (report.RiskLevel)
            {
                case "High":
                    warnings.Add("⚠️ هذه الميزة عالية الخطورة — التغيير قد يؤثر على عمليات حساسة.");
                    break;
                case "Medium":
                    warnings.Add("⚡ هذه الميزة متوسطة الخطورة.");
                    break;
                case "Low":
                    warnings.Add("✅ هذه الميزة منخفضة الخطورة.");
                    break;
            }

            // Migration warning
            if (report.RequiresMigration)
            {
                warnings.Add("🔧 هذه الميزة تتطلب Migration — لا يمكن التفعيل بدون تحديث قاعدة البيانات.");
            }

            // Impact areas
            if (report.ImpactAreas.Count > 0)
            {
                warnings.Add($"📌 المناطق المتأثرة: {string.Join("، ", report.ImpactAreas)}");
            }

            // Impact description
            if (!string.IsNullOrWhiteSpace(feature.ImpactDescription))
            {
                warnings.Add($"💡 {feature.ImpactDescription}");
            }

            // Disabled dependencies
            if (report.DisabledDependencies.Count > 0)
            {
                warnings.Add($"🚫 تبعيات غير مفعلة: {string.Join("، ", report.DisabledDependencies)} — يجب تفعيلها أولاً.");
            }

            // Dependencies info
            if (report.Dependencies.Count > 0 && report.DisabledDependencies.Count == 0)
            {
                warnings.Add($"🔗 تعتمد على: {string.Join("، ", report.Dependencies)}");
            }

            return string.Join("\n", warnings);
        }
    }
}

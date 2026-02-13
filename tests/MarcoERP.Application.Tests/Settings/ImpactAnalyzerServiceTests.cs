using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MarcoERP.Application.Services.Settings;
using MarcoERP.Domain.Entities.Settings;
using MarcoERP.Domain.Interfaces.Settings;
using Moq;
using Xunit;

namespace MarcoERP.Application.Tests.Settings
{
    /// <summary>
    /// Phase 4F: Impact Analyzer tests — 3 scenarios.
    /// </summary>
    public sealed class ImpactAnalyzerServiceTests
    {
        private readonly Mock<IFeatureRepository> _featureRepoMock;
        private readonly ImpactAnalyzerService _sut;

        public ImpactAnalyzerServiceTests()
        {
            _featureRepoMock = new Mock<IFeatureRepository>();
            _sut = new ImpactAnalyzerService(_featureRepoMock.Object);
        }

        // ── Helper ───────────────────────────────────────────────

        private static Feature CreateFeature(
            string key, string riskLevel, bool isEnabled,
            string dependsOn = null,
            bool affectsData = false, bool requiresMigration = false,
            bool affectsAccounting = false, bool affectsInventory = false,
            bool affectsReporting = false, string impactDescription = null)
        {
            var feature = new Feature(key, key, key, $"Test {key}", isEnabled, riskLevel, dependsOn);
            feature.SetImpactMetadata(affectsData, requiresMigration, affectsAccounting, affectsInventory, affectsReporting, impactDescription);
            return feature;
        }

        // ── Test 1: Low Risk → can proceed directly ──────────────

        [Fact]
        public async Task AnalyzeAsync_LowRisk_CanProceed()
        {
            // Arrange: Low risk feature, no dependencies
            var feature = CreateFeature("POS", "Low", isEnabled: true,
                dependsOn: null, affectsData: false, affectsReporting: false,
                impactDescription: "نقاط البيع واجهة فقط — التعطيل آمن");

            _featureRepoMock.Setup(r => r.GetByKeyAsync("POS", It.IsAny<CancellationToken>()))
                .ReturnsAsync(feature);

            // Act
            var report = await _sut.AnalyzeAsync("POS");

            // Assert
            report.FeatureKey.Should().Be("POS");
            report.RiskLevel.Should().Be("Low");
            report.CanProceed.Should().BeTrue();
            report.RequiresMigration.Should().BeFalse();
            report.DisabledDependencies.Should().BeEmpty();
            report.WarningMessage.Should().Contain("منخفضة الخطورة");
        }

        // ── Test 2: High Risk → warning with impact areas ────────

        [Fact]
        public async Task AnalyzeAsync_HighRisk_ReturnsWarningWithImpactAreas()
        {
            // Arrange: High risk feature affecting accounting + reporting
            var feature = CreateFeature("Accounting", "High", isEnabled: true,
                affectsData: true, affectsAccounting: true, affectsReporting: true,
                impactDescription: "تعطيل المحاسبة يؤثر على كل القيود والتقارير المالية");

            _featureRepoMock.Setup(r => r.GetByKeyAsync("Accounting", It.IsAny<CancellationToken>()))
                .ReturnsAsync(feature);

            // Act
            var report = await _sut.AnalyzeAsync("Accounting");

            // Assert
            report.RiskLevel.Should().Be("High");
            report.CanProceed.Should().BeTrue(); // Already enabled, no deps to check
            report.ImpactAreas.Should().Contain("المحاسبة (Accounting)");
            report.ImpactAreas.Should().Contain("التقارير (Reporting)");
            report.ImpactAreas.Should().Contain("البيانات المخزنة (Stored Data)");
            report.WarningMessage.Should().Contain("عالية الخطورة");
            report.WarningMessage.Should().Contain("تعطيل المحاسبة يؤثر");
        }

        // ── Test 3: Disabled dependency → blocked ────────────────

        [Fact]
        public async Task AnalyzeAsync_DisabledDependency_BlocksEnabling()
        {
            // Arrange: POS (disabled) depends on Sales + Treasury
            // Sales is enabled, but Treasury is disabled
            var posFeature = CreateFeature("POS", "Low", isEnabled: false,
                dependsOn: "Sales,Treasury",
                affectsData: false, affectsAccounting: true, affectsInventory: true);

            var allFeatures = new List<Feature>
            {
                CreateFeature("Sales", "Medium", isEnabled: true),
                CreateFeature("Treasury", "Medium", isEnabled: false),    // ← disabled dep
                CreateFeature("Accounting", "High", isEnabled: true),
                posFeature
            };

            _featureRepoMock.Setup(r => r.GetByKeyAsync("POS", It.IsAny<CancellationToken>()))
                .ReturnsAsync(posFeature);
            _featureRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(allFeatures);

            // Act
            var report = await _sut.AnalyzeAsync("POS");

            // Assert
            report.CanProceed.Should().BeFalse();
            report.DisabledDependencies.Should().ContainSingle("Treasury");
            report.WarningMessage.Should().Contain("تبعيات غير مفعلة");
            report.WarningMessage.Should().Contain("Treasury");
        }

        // ── Test 4: Feature not found → CanProceed = false ───────

        [Fact]
        public async Task AnalyzeAsync_FeatureNotFound_ReturnsFalseCanProceed()
        {
            _featureRepoMock.Setup(r => r.GetByKeyAsync("NonExistent", It.IsAny<CancellationToken>()))
                .ReturnsAsync((Feature)null);

            var report = await _sut.AnalyzeAsync("NonExistent");

            report.CanProceed.Should().BeFalse();
            report.RiskLevel.Should().Be("Unknown");
            report.WarningMessage.Should().Contain("غير موجودة");
        }

        // ── Test 5: RequiresMigration → warning message ──────────

        [Fact]
        public async Task AnalyzeAsync_RequiresMigration_WarnsAboutMigration()
        {
            var feature = CreateFeature("CustomModule", "Medium", isEnabled: false,
                requiresMigration: true, impactDescription: "يتطلب تحديث الجداول");

            _featureRepoMock.Setup(r => r.GetByKeyAsync("CustomModule", It.IsAny<CancellationToken>()))
                .ReturnsAsync(feature);
            _featureRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Feature> { feature });

            var report = await _sut.AnalyzeAsync("CustomModule");

            report.RequiresMigration.Should().BeTrue();
            report.WarningMessage.Should().Contain("Migration");
        }

        // ── Test 6: All dependencies enabled → CanProceed ────────

        [Fact]
        public async Task AnalyzeAsync_AllDependenciesEnabled_CanProceed()
        {
            var salesFeature = CreateFeature("Sales", "Medium", isEnabled: false,
                dependsOn: "Accounting,Inventory");

            var allFeatures = new List<Feature>
            {
                CreateFeature("Accounting", "High", isEnabled: true),
                CreateFeature("Inventory", "Medium", isEnabled: true),
                salesFeature
            };

            _featureRepoMock.Setup(r => r.GetByKeyAsync("Sales", It.IsAny<CancellationToken>()))
                .ReturnsAsync(salesFeature);
            _featureRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(allFeatures);

            var report = await _sut.AnalyzeAsync("Sales");

            report.CanProceed.Should().BeTrue();
            report.DisabledDependencies.Should().BeEmpty();
            report.Dependencies.Should().Contain("Accounting");
            report.Dependencies.Should().Contain("Inventory");
        }
    }
}

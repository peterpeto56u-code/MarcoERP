using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MarcoERP.Application.Common;
using MarcoERP.Application.DTOs.Settings;
using MarcoERP.Application.Interfaces.Settings;
using MarcoERP.Domain.Enums;

namespace MarcoERP.WpfUI.ViewModels.Settings
{
    /// <summary>
    /// ViewModel for the Governance Console screen (وحدة التحكم).
    /// Phase 2: Feature Governance Engine — read/toggle features.
    /// Phase 3: Profile Selection — apply complexity profiles.
    /// Phase 4: Impact Analyzer — pre-toggle analysis + dependency blocking.
    /// Phase 8F: Module Dependency Graph visualization.
    /// </summary>
    public sealed class GovernanceConsoleViewModel : BaseViewModel
    {
        private readonly IFeatureService _featureService;
        private readonly IProfileService _profileService;
        private readonly IImpactAnalyzerService _impactAnalyzer;
        private readonly IServiceProvider _serviceProvider;
        private readonly IModuleDependencyInspector _dependencyInspector;

        public GovernanceConsoleViewModel(
            IFeatureService featureService,
            IProfileService profileService,
            IImpactAnalyzerService impactAnalyzer,
            IServiceProvider serviceProvider,
            IModuleDependencyInspector dependencyInspector = null)
        {
            _featureService = featureService ?? throw new ArgumentNullException(nameof(featureService));
            _profileService = profileService ?? throw new ArgumentNullException(nameof(profileService));
            _impactAnalyzer = impactAnalyzer ?? throw new ArgumentNullException(nameof(impactAnalyzer));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _dependencyInspector = dependencyInspector;

            Features = new ObservableCollection<FeatureRowViewModel>();
            ProfileNames = new ObservableCollection<string> { "Simple", "Standard", "Advanced" };
            DependencyRows = new ObservableCollection<DependencyGraphRow>();

            LoadCommand = new AsyncRelayCommand(LoadAllAsync);
            ApplyProfileCommand = new AsyncRelayCommand(ApplySelectedProfileAsync);
        }

        // ── Collections ──────────────────────────────────────────

        public ObservableCollection<FeatureRowViewModel> Features { get; }
        public ObservableCollection<string> ProfileNames { get; }

        /// <summary>Phase 8F: Module dependency graph rows.</summary>
        public ObservableCollection<DependencyGraphRow> DependencyRows { get; }

        // ── Profile Selection ────────────────────────────────────

        private string _selectedProfile;
        public string SelectedProfile
        {
            get => _selectedProfile;
            set => SetProperty(ref _selectedProfile, value);
        }

        private string _currentProfileDisplay;
        public string CurrentProfileDisplay
        {
            get => _currentProfileDisplay;
            set => SetProperty(ref _currentProfileDisplay, value);
        }

        // ── Commands ─────────────────────────────────────────────

        public ICommand LoadCommand { get; }
        public ICommand ApplyProfileCommand { get; }

        // ── Load ─────────────────────────────────────────────────

        private async Task LoadAllAsync()
        {
            IsBusy = true;
            ClearError();
            try
            {
                // Load features
                var featResult = await _featureService.GetAllAsync();
                if (featResult.IsSuccess)
                {
                    Features.Clear();
                    foreach (var dto in featResult.Data)
                    {
                        var row = new FeatureRowViewModel(dto, ToggleFeatureAsync);
                        Features.Add(row);
                    }
                }
                else
                {
                    ErrorMessage = featResult.ErrorMessage;
                    return;
                }

                // Load current profile
                var profileResult = await _profileService.GetCurrentProfileAsync();
                if (profileResult.IsSuccess)
                {
                    CurrentProfileDisplay = profileResult.Data;
                    SelectedProfile = profileResult.Data;
                }

                StatusMessage = $"تم تحميل {Features.Count} ميزة — البروفايل الحالي: {CurrentProfileDisplay ?? "غير محدد"}";

                // Phase 8F: Load dependency graph
                LoadDependencyGraph();
            }
            catch (Exception ex)
            {
                ErrorMessage = FriendlyErrorMessage("التحميل", ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        // ── Toggle ───────────────────────────────────────────────

        private async Task ToggleFeatureAsync(FeatureRowViewModel row)
        {
            bool isEnabling = !row.IsEnabled;
            string action = isEnabling ? "تفعيل" : "تعطيل";

            // ── Phase 4: Impact Analysis before toggle ──────────
            IsBusy = true;
            ClearError();
            FeatureImpactReport report;
            try
            {
                report = await _impactAnalyzer.AnalyzeAsync(row.FeatureKey);
            }
            catch (Exception ex)
            {
                ErrorMessage = FriendlyErrorMessage("تحليل التأثير", ex);
                IsBusy = false;
                return;
            }
            finally
            {
                IsBusy = false;
            }

            // ── Phase 4E: Block if dependencies are disabled ────
            if (isEnabling && !report.CanProceed && report.DisabledDependencies.Count > 0)
            {
                MessageBox.Show(
                    $"🚫 لا يمكن تفعيل '{row.NameAr}'\n\n" +
                    $"التبعيات التالية غير مفعلة:\n" +
                    $"  • {string.Join("\n  • ", report.DisabledDependencies)}\n\n" +
                    "يجب تفعيل هذه الميزات أولاً.",
                    "تبعيات غير مفعلة",
                    MessageBoxButton.OK,
                    MessageBoxImage.Stop);
                return;
            }

            // ── Show Impact Report ──────────────────────────────
            string reportText =
                $"📊 تقرير تأثير {action} '{row.NameAr}'\n" +
                $"━━━━━━━━━━━━━━━━━━━━━━━━━━━\n" +
                $"مستوى الخطورة: {report.RiskLevel}\n" +
                (report.RequiresMigration ? "⚠️ يتطلب Migration\n" : "") +
                (report.ImpactAreas.Count > 0 ? $"المناطق المتأثرة: {string.Join("، ", report.ImpactAreas)}\n" : "") +
                (report.Dependencies.Count > 0 ? $"يعتمد على: {string.Join("، ", report.Dependencies)}\n" : "") +
                $"━━━━━━━━━━━━━━━━━━━━━━━━━━━\n" +
                report.WarningMessage +
                $"\n\nهل تريد المتابعة بعملية {action}؟";

            var firstConfirmation = MessageBox.Show(
                reportText,
                $"تقرير التأثير — {action} '{row.NameAr}'",
                MessageBoxButton.YesNo,
                report.RiskLevel == "High" ? MessageBoxImage.Warning : MessageBoxImage.Question);

            if (firstConfirmation != MessageBoxResult.Yes)
                return;

            // ── Phase 4D: Double confirmation for High risk ─────
            if (report.RiskLevel == "High")
            {
                var secondConfirmation = MessageBox.Show(
                    $"⚠️⚠️ تأكيد نهائي ⚠️⚠️\n\n" +
                    $"أنت على وشك {action} ميزة عالية الخطورة: '{row.NameAr}'\n\n" +
                    "هذا الإجراء قد يؤثر على عمليات حساسة في النظام.\n\n" +
                    "هل أنت متأكد تماماً؟",
                    "تأكيد نهائي — ميزة عالية الخطورة",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Exclamation);

                if (secondConfirmation != MessageBoxResult.Yes)
                    return;
            }

            // ── Execute toggle ──────────────────────────────────
            IsBusy = true;
            ClearError();
            try
            {
                var dto = new ToggleFeatureDto
                {
                    FeatureKey = row.FeatureKey,
                    IsEnabled = isEnabling
                };

                var result = await _featureService.ToggleAsync(dto);
                if (result.IsSuccess)
                {
                    row.IsEnabled = dto.IsEnabled;
                    StatusMessage = $"تم {action} '{row.NameAr}' — {report.RiskLevel} Risk";
                    await RefreshMainNavigationAsync();
                }
                else
                {
                    ErrorMessage = result.ErrorMessage;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = FriendlyErrorMessage("التبديل", ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        // ── Apply Profile ────────────────────────────────────────

        private async Task ApplySelectedProfileAsync()
        {
            if (string.IsNullOrWhiteSpace(SelectedProfile))
            {
                ErrorMessage = "اختر بروفايل أولاً.";
                return;
            }

            // Confirmation dialog
            var confirmation = MessageBox.Show(
                $"هل تريد تطبيق البروفايل '{SelectedProfile}'؟\n\n" +
                "⚠️ سيتم تفعيل/تعطيل الميزات حسب البروفايل المختار.\n" +
                "💾 يُنصح بأخذ نسخة احتياطية قبل التغيير.\n\n" +
                "لن يتم حذف أي بيانات — فقط تغيير ظهور الشاشات.",
                "تأكيد تغيير البروفايل",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirmation != MessageBoxResult.Yes)
                return;

            IsBusy = true;
            ClearError();
            try
            {
                var result = await _profileService.ApplyProfileAsync(SelectedProfile);
                if (result.IsSuccess)
                {
                    CurrentProfileDisplay = SelectedProfile;
                    StatusMessage = $"تم تطبيق البروفايل '{SelectedProfile}' بنجاح";
                    // Reload features to reflect changes
                    await LoadAllAsync();
                    // Refresh main window navigation
                    await RefreshMainNavigationAsync();
                }
                else
                {
                    ErrorMessage = result.ErrorMessage;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = FriendlyErrorMessage("تطبيق البروفايل", ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        // ── Navigation Refresh ───────────────────────────────────

        private async Task RefreshMainNavigationAsync()
        {
            try
            {
                var mainVm = _serviceProvider.GetService(typeof(Shell.MainWindowViewModel)) as Shell.MainWindowViewModel;
                if (mainVm != null)
                    await mainVm.RefreshNavigationAsync();
            }
            catch
            {
                // Best-effort refresh — sidebar will update on next app restart
            }
        }
        // ── Phase 8F: Dependency Graph ──────────────────────────

        private void LoadDependencyGraph()
        {
            DependencyRows.Clear();

            // Build allowed-dependency rows from ModuleRegistry
            var violations = _dependencyInspector?.ValidateDependencies() ?? new List<ModuleDependencyViolation>();
            var violationSet = new HashSet<string>(
                violations.Select(v => $"{v.SourceModule}→{v.DependencyModule}"));

            foreach (var def in ModuleRegistry.Definitions)
            {
                if (def.Module == SystemModule.Common) continue;

                // Self row (no dependency — just shows the module exists)
                if (def.AllowedDependencies.Count == 0)
                {
                    DependencyRows.Add(new DependencyGraphRow
                    {
                        Module = GetModuleArabicName(def.Module),
                        DependsOn = "—",
                        IsAllowed = true
                    });
                    continue;
                }

                foreach (var dep in def.AllowedDependencies)
                {
                    DependencyRows.Add(new DependencyGraphRow
                    {
                        Module = GetModuleArabicName(def.Module),
                        DependsOn = GetModuleArabicName(dep),
                        IsAllowed = true
                    });
                }
            }

            // Add violation rows (unauthorized dependencies — red)
            foreach (var v in violations)
            {
                // Avoid duplicates
                var exists = DependencyRows.Any(r =>
                    r.Module == GetModuleArabicName(Enum.Parse<SystemModule>(v.SourceModule)) &&
                    r.DependsOn == GetModuleArabicName(Enum.Parse<SystemModule>(v.DependencyModule)));
                if (!exists)
                {
                    DependencyRows.Add(new DependencyGraphRow
                    {
                        Module = GetModuleArabicName(Enum.Parse<SystemModule>(v.SourceModule)),
                        DependsOn = GetModuleArabicName(Enum.Parse<SystemModule>(v.DependencyModule)),
                        IsAllowed = false,
                        ViolationDetail = v.Message
                    });
                }
            }
        }

        private static string GetModuleArabicName(SystemModule module) => module switch
        {
            SystemModule.Sales => "المبيعات",
            SystemModule.Inventory => "المخزون",
            SystemModule.Accounting => "المحاسبة",
            SystemModule.Purchases => "المشتريات",
            SystemModule.Treasury => "الخزينة",
            SystemModule.Reporting => "التقارير",
            SystemModule.Security => "الأمان",
            SystemModule.Settings => "الإعدادات",
            SystemModule.Governance => "الحوكمة",
            SystemModule.Common => "عام",
            _ => module.ToString()
        };
    }

    /// <summary>
    /// Phase 8F: A row in the dependency graph table.
    /// </summary>
    public sealed class DependencyGraphRow
    {
        public string Module { get; set; }
        public string DependsOn { get; set; }
        public bool IsAllowed { get; set; }
        public string Status => IsAllowed ? "مصرح" : "غير مصرح";
        public string ViolationDetail { get; set; }
    }

    /// <summary>
    /// Row-level ViewModel wrapping a FeatureDto for DataGrid binding with toggle support.
    /// </summary>
    public sealed class FeatureRowViewModel : BaseViewModel
    {
        private readonly Func<FeatureRowViewModel, Task> _toggleCallback;

        public FeatureRowViewModel(FeatureDto dto, Func<FeatureRowViewModel, Task> toggleCallback)
        {
            _toggleCallback = toggleCallback;
            Id = dto.Id;
            FeatureKey = dto.FeatureKey;
            NameAr = dto.NameAr;
            NameEn = dto.NameEn;
            Description = dto.Description;
            _isEnabled = dto.IsEnabled;
            RiskLevel = dto.RiskLevel;
            DependsOn = dto.DependsOn;

            ToggleCommand = new AsyncRelayCommand(() => _toggleCallback(this));
        }

        public int Id { get; }
        public string FeatureKey { get; }
        public string NameAr { get; }
        public string NameEn { get; }
        public string Description { get; }
        public string RiskLevel { get; }
        public string DependsOn { get; }

        private bool _isEnabled;
        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }

        public ICommand ToggleCommand { get; }
    }
}

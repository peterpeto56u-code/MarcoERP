using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using MarcoERP.Application.Interfaces;
using MarcoERP.Application.Interfaces.Settings;

namespace MarcoERP.WpfUI.ViewModels.Settings
{
    /// <summary>
    /// ViewModel for the Governance Integrity Check screen (فحص سلامة الحوكمة).
    /// Phase 5: Version &amp; Integrity Engine — read-only checks, no blocking.
    /// </summary>
    public sealed class GovernanceIntegrityViewModel : BaseViewModel
    {
        private readonly IIntegrityCheckService _integrityCheckService;
        private readonly IVersionService _versionService;
        private readonly IDateTimeProvider _dateTimeProvider;

        public GovernanceIntegrityViewModel(
            IIntegrityCheckService integrityCheckService,
            IVersionService versionService,
            IDateTimeProvider dateTimeProvider)
        {
            _integrityCheckService = integrityCheckService ?? throw new ArgumentNullException(nameof(integrityCheckService));
            _versionService = versionService ?? throw new ArgumentNullException(nameof(versionService));
            _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));

            CheckResults = new ObservableCollection<IntegrityCheckRowViewModel>();

            RunChecksCommand = new AsyncRelayCommand(ExecuteRunChecksAsync);
            LoadCommand = new AsyncRelayCommand(ExecuteRunChecksAsync);
        }

        // ── Collections ──────────────────────────────────────────

        public ObservableCollection<IntegrityCheckRowViewModel> CheckResults { get; }

        // ── Commands ─────────────────────────────────────────────

        public ICommand RunChecksCommand { get; }
        public ICommand LoadCommand { get; }

        // ── Properties ──────────────────────────────────────────

        private string _currentDbVersion;
        public string CurrentDbVersion
        {
            get => _currentDbVersion;
            set => SetProperty(ref _currentDbVersion, value);
        }

        private string _currentCodeVersion;
        public string CurrentCodeVersion
        {
            get => _currentCodeVersion;
            set => SetProperty(ref _currentCodeVersion, value);
        }

        private string _overallStatus;
        public string OverallStatus
        {
            get => _overallStatus;
            set => SetProperty(ref _overallStatus, value);
        }

        private bool _hasResults;
        public bool HasResults
        {
            get => _hasResults;
            set => SetProperty(ref _hasResults, value);
        }

        private string _checkDate;
        public string CheckDate
        {
            get => _checkDate;
            set => SetProperty(ref _checkDate, value);
        }

        // ── Execute ─────────────────────────────────────────────

        private async Task ExecuteRunChecksAsync()
        {
            IsBusy = true;
            ClearError();
            HasResults = false;
            CheckResults.Clear();

            try
            {
                StatusMessage = "جاري تشغيل فحوصات الحوكمة...";

                // Get versions
                CurrentDbVersion = await _versionService.GetCurrentVersionAsync();
                CurrentCodeVersion = App.CurrentAppVersion;

                // Run all checks
                var results = await _integrityCheckService.RunChecksAsync();

                bool hasCritical = false;
                bool hasWarning = false;

                foreach (var result in results)
                {
                    CheckResults.Add(new IntegrityCheckRowViewModel
                    {
                        CheckName = result.CheckName,
                        Status = result.Status,
                        Message = result.Message
                    });

                    if (result.Status == "Critical") hasCritical = true;
                    if (result.Status == "Warning") hasWarning = true;
                }

                CheckDate = _dateTimeProvider.UtcNow.ToString("yyyy/MM/dd HH:mm:ss");

                if (hasCritical)
                    OverallStatus = "يوجد مشاكل حرجة ⚠️";
                else if (hasWarning)
                    OverallStatus = "يوجد تحذيرات ⚡";
                else
                    OverallStatus = "سليم ✓";

                HasResults = true;
                StatusMessage = $"تم تنفيذ {results.Count} فحص — {OverallStatus}";
            }
            catch (Exception ex)
            {
                ErrorMessage = FriendlyErrorMessage("فحص السلامة", ex);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }

    /// <summary>
    /// Row ViewModel for a single integrity check result.
    /// </summary>
    public sealed class IntegrityCheckRowViewModel : BaseViewModel
    {
        public string CheckName { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
    }
}

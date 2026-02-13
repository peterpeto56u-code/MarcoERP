using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using MarcoERP.Application.DTOs.Settings;
using MarcoERP.Application.Interfaces.Settings;

namespace MarcoERP.WpfUI.ViewModels.Settings
{
    /// <summary>
    /// ViewModel for the Migration Center screen (مركز التحديثات).
    /// Phase 6: Controlled Migration Engine — displays pending migrations, history,
    /// and drives safe execution (backup → migrate → log).
    /// Safety Rules: disables UI during execution, shows progress, logs errors.
    /// </summary>
    public sealed class MigrationCenterViewModel : BaseViewModel
    {
        private readonly IMigrationExecutionService _migrationService;

        public MigrationCenterViewModel(IMigrationExecutionService migrationService)
        {
            _migrationService = migrationService ?? throw new ArgumentNullException(nameof(migrationService));

            PendingMigrations = new ObservableCollection<string>();
            ExecutionHistory = new ObservableCollection<MigrationExecutionDto>();

            LoadCommand = new AsyncRelayCommand(LoadAsync);
            ExecuteMigrationsCommand = new AsyncRelayCommand(ExecuteMigrationsAsync, () => CanExecute);
        }

        // ── Collections ──────────────────────────────────────────

        public ObservableCollection<string> PendingMigrations { get; }
        public ObservableCollection<MigrationExecutionDto> ExecutionHistory { get; }

        // ── Commands ─────────────────────────────────────────────

        public ICommand LoadCommand { get; }
        public ICommand ExecuteMigrationsCommand { get; }

        // ── Properties ──────────────────────────────────────────

        private int _pendingCount;
        public int PendingCount
        {
            get => _pendingCount;
            set
            {
                if (SetProperty(ref _pendingCount, value))
                    OnPropertyChanged(nameof(HasPending));
            }
        }

        public bool HasPending => PendingCount > 0;

        private bool _isExecuting;
        public bool IsExecuting
        {
            get => _isExecuting;
            set
            {
                if (SetProperty(ref _isExecuting, value))
                    OnPropertyChanged(nameof(CanExecute));
            }
        }

        public bool CanExecute => HasPending && !IsExecuting;

        private string _statusMessage;
        public new string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        private bool _lastExecutionSucceeded;
        public bool LastExecutionSucceeded
        {
            get => _lastExecutionSucceeded;
            set => SetProperty(ref _lastExecutionSucceeded, value);
        }

        private bool _hasResult;
        public bool HasResult
        {
            get => _hasResult;
            set => SetProperty(ref _hasResult, value);
        }

        private string _progressText;
        public string ProgressText
        {
            get => _progressText;
            set => SetProperty(ref _progressText, value);
        }

        // ── Methods ─────────────────────────────────────────────

        private async Task LoadAsync()
        {
            try
            {
                IsExecuting = false;
                HasResult = false;
                StatusMessage = "جارٍ التحميل...";

                var pending = await _migrationService.GetPendingMigrationsAsync();
                PendingMigrations.Clear();
                foreach (var m in pending)
                    PendingMigrations.Add(m);
                PendingCount = pending.Count;

                var history = await _migrationService.GetExecutionHistoryAsync();
                ExecutionHistory.Clear();
                foreach (var h in history)
                    ExecutionHistory.Add(h);

                StatusMessage = PendingCount > 0
                    ? $"يوجد {PendingCount} تحديث معلق"
                    : "قاعدة البيانات محدّثة ✓";
            }
            catch (Exception ex)
            {
                StatusMessage = FriendlyErrorMessage("التحميل", ex);
            }
        }

        private async Task ExecuteMigrationsAsync()
        {
            if (!CanExecute) return;

            try
            {
                // ── Safety Rule: Disable UI during execution ──
                IsExecuting = true;
                HasResult = false;
                ProgressText = "جارٍ إنشاء نسخة احتياطية...";
                StatusMessage = "تنفيذ التحديثات — لا تغلق التطبيق";

                ProgressText = "جارٍ تنفيذ الترحيل...";

                var result = await _migrationService.ExecuteMigrationsAsync();

                if (result.IsSuccess)
                {
                    LastExecutionSucceeded = true;
                    StatusMessage = "تم تنفيذ جميع التحديثات بنجاح ✓";
                    ProgressText = "اكتمل بنجاح";
                }
                else
                {
                    LastExecutionSucceeded = false;
                    StatusMessage = $"فشل التنفيذ: {result.ErrorMessage}";
                    ProgressText = "فشل التنفيذ";
                }

                HasResult = true;
            }
            catch (Exception ex)
            {
                LastExecutionSucceeded = false;
                StatusMessage = FriendlyErrorMessage("العملية", ex);
                ProgressText = "خطأ";
                HasResult = true;
            }
            finally
            {
                IsExecuting = false;
                // Reload to refresh pending list and history
                await LoadAsync();
            }
        }
    }
}

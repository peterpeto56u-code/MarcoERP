using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using MarcoERP.Application.DTOs.Settings;
using MarcoERP.Application.Interfaces.Settings;

namespace MarcoERP.WpfUI.ViewModels.Settings
{
    /// <summary>
    /// ViewModel for the Data Integrity Check screen (فحص سلامة البيانات).
    /// Phase C.2: Trial Balance, Journal Balance, Inventory Reconciliation.
    /// </summary>
    public sealed class IntegrityCheckViewModel : BaseViewModel
    {
        private readonly IIntegrityService _integrityService;

        public IntegrityCheckViewModel(IIntegrityService integrityService)
        {
            _integrityService = integrityService ?? throw new ArgumentNullException(nameof(integrityService));

            UnbalancedAccounts = new ObservableCollection<UnbalancedAccountDto>();
            UnbalancedEntries = new ObservableCollection<UnbalancedJournalEntryDto>();
            InventoryInconsistencies = new ObservableCollection<InventoryInconsistencyDto>();

            RunFullCheckCommand = new AsyncRelayCommand(ExecuteRunFullCheckAsync);
        }

        // ── Collections ──────────────────────────────────────────

        public ObservableCollection<UnbalancedAccountDto> UnbalancedAccounts { get; }
        public ObservableCollection<UnbalancedJournalEntryDto> UnbalancedEntries { get; }
        public ObservableCollection<InventoryInconsistencyDto> InventoryInconsistencies { get; }

        // ── Commands ─────────────────────────────────────────────

        public ICommand RunFullCheckCommand { get; }

        // ── Properties ──────────────────────────────────────────

        private bool _isRunning;
        public bool IsRunning
        {
            get => _isRunning;
            set => SetProperty(ref _isRunning, value);
        }

        private string _overallStatus;
        public string OverallStatus
        {
            get => _overallStatus;
            set => SetProperty(ref _overallStatus, value);
        }

        private string _checkDate;
        public string CheckDate
        {
            get => _checkDate;
            set => SetProperty(ref _checkDate, value);
        }

        // ── Trial Balance ───────────────────────────────────────

        private bool _trialBalancePassed;
        public bool TrialBalancePassed
        {
            get => _trialBalancePassed;
            set => SetProperty(ref _trialBalancePassed, value);
        }

        private string _trialBalanceSummary;
        public string TrialBalanceSummary
        {
            get => _trialBalanceSummary;
            set => SetProperty(ref _trialBalanceSummary, value);
        }

        // ── Journal Balance ─────────────────────────────────────

        private bool _journalBalancePassed;
        public bool JournalBalancePassed
        {
            get => _journalBalancePassed;
            set => SetProperty(ref _journalBalancePassed, value);
        }

        private string _journalBalanceSummary;
        public string JournalBalanceSummary
        {
            get => _journalBalanceSummary;
            set => SetProperty(ref _journalBalanceSummary, value);
        }

        // ── Inventory ───────────────────────────────────────────

        private bool _inventoryPassed;
        public bool InventoryPassed
        {
            get => _inventoryPassed;
            set => SetProperty(ref _inventoryPassed, value);
        }

        private string _inventorySummary;
        public string InventorySummary
        {
            get => _inventorySummary;
            set => SetProperty(ref _inventorySummary, value);
        }

        // ── Check Completed Flag ────────────────────────────────

        private bool _hasResults;
        public bool HasResults
        {
            get => _hasResults;
            set => SetProperty(ref _hasResults, value);
        }

        // ── Execute Full Check ──────────────────────────────────

        private async Task ExecuteRunFullCheckAsync()
        {
            IsRunning = true;
            IsBusy = true;
            ClearError();
            HasResults = false;
            OverallStatus = null;

            try
            {
                StatusMessage = "جاري فحص سلامة البيانات...";

                var result = await _integrityService.RunFullCheckAsync();

                if (result.IsSuccess)
                {
                    var report = result.Data;

                    CheckDate = report.CheckDate.ToString("yyyy/MM/dd HH:mm:ss");

                    // Trial Balance
                    TrialBalancePassed = report.TrialBalance.IsBalanced;
                    TrialBalanceSummary = report.TrialBalance.IsBalanced
                        ? $"متوازن — إجمالي المدين: {report.TrialBalance.TotalDebits:N2} | إجمالي الدائن: {report.TrialBalance.TotalCredits:N2}"
                        : $"غير متوازن — الفرق: {report.TrialBalance.Difference:N2} | حسابات غير متوازنة: {report.TrialBalance.UnbalancedAccounts.Count}";

                    UnbalancedAccounts.Clear();
                    foreach (var item in report.TrialBalance.UnbalancedAccounts)
                        UnbalancedAccounts.Add(item);

                    // Journal Balance
                    JournalBalancePassed = report.JournalBalance.AllBalanced;
                    JournalBalanceSummary = report.JournalBalance.AllBalanced
                        ? $"جميع القيود متوازنة — تم فحص {report.JournalBalance.TotalChecked} قيد"
                        : $"قيود غير متوازنة: {report.JournalBalance.UnbalancedCount} من أصل {report.JournalBalance.TotalChecked}";

                    UnbalancedEntries.Clear();
                    foreach (var item in report.JournalBalance.UnbalancedEntries)
                        UnbalancedEntries.Add(item);

                    // Inventory
                    InventoryPassed = report.Inventory.IsConsistent;
                    InventorySummary = report.Inventory.IsConsistent
                        ? $"المخزون متطابق — تم فحص {report.Inventory.TotalProductsChecked} سجل"
                        : $"عدم تطابق: {report.Inventory.InconsistentCount} من أصل {report.Inventory.TotalProductsChecked}";

                    InventoryInconsistencies.Clear();
                    foreach (var item in report.Inventory.Inconsistencies)
                        InventoryInconsistencies.Add(item);

                    // Overall
                    OverallStatus = report.OverallHealthy
                        ? "سليم ✓"
                        : "توجد مشكلات ✗";

                    HasResults = true;
                    StatusMessage = "اكتمل الفحص.";
                }
                else
                {
                    ErrorMessage = result.ErrorMessage;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = FriendlyErrorMessage("الفحص", ex);
            }
            finally
            {
                IsRunning = false;
                IsBusy = false;
            }
        }
    }
}

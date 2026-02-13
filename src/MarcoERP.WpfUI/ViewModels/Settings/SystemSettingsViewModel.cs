using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using MarcoERP.Application.DTOs.Settings;
using MarcoERP.Application.Interfaces.Settings;

namespace MarcoERP.WpfUI.ViewModels.Settings
{
    /// <summary>
    /// ViewModel for System Settings screen (إعدادات النظام).
    /// Phase 5D: Grouped key-value settings with batch save.
    /// </summary>
    public sealed class SystemSettingsViewModel : BaseViewModel
    {
        private readonly ISystemSettingsService _settingsService;

        public SystemSettingsViewModel(ISystemSettingsService settingsService)
        {
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));

            AllSettings = new ObservableCollection<SystemSettingDto>();
            GroupNames = new ObservableCollection<string>();
            FilteredSettings = new ObservableCollection<SystemSettingDto>();

            LoadCommand = new AsyncRelayCommand(LoadSettingsAsync);
            SaveAllCommand = new AsyncRelayCommand(SaveAllAsync);
        }

        // ── Collections ──────────────────────────────────────────

        public ObservableCollection<SystemSettingDto> AllSettings { get; }
        public ObservableCollection<string> GroupNames { get; }
        public ObservableCollection<SystemSettingDto> FilteredSettings { get; }

        // ── Selected Group ──────────────────────────────────────

        private string _selectedGroup;
        public string SelectedGroup
        {
            get => _selectedGroup;
            set
            {
                if (SetProperty(ref _selectedGroup, value))
                    ApplyGroupFilter();
            }
        }

        // ── Commands ─────────────────────────────────────────────

        public ICommand LoadCommand { get; }
        public ICommand SaveAllCommand { get; }

        // ── Load ─────────────────────────────────────────────────

        public async Task LoadSettingsAsync()
        {
            IsBusy = true;
            ClearError();
            try
            {
                var result = await _settingsService.GetAllAsync();
                if (result.IsSuccess)
                {
                    AllSettings.Clear();
                    GroupNames.Clear();
                    FilteredSettings.Clear();

                    foreach (var s in result.Data)
                        AllSettings.Add(s);

                    var groups = result.Data.Select(s => s.GroupName).Distinct().OrderBy(g => g).ToList();
                    groups.Insert(0, "الكل");
                    foreach (var g in groups)
                        GroupNames.Add(g);

                    SelectedGroup = "الكل";
                    StatusMessage = $"تم تحميل {AllSettings.Count} إعداد";
                }
                else
                {
                    ErrorMessage = result.ErrorMessage;
                }
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

        // ── Save All ─────────────────────────────────────────────

        private async Task SaveAllAsync()
        {
            IsBusy = true;
            ClearError();
            try
            {
                var updates = FilteredSettings
                    .Select(s => new UpdateSystemSettingDto
                    {
                        SettingKey = s.SettingKey,
                        SettingValue = s.SettingValue
                    })
                    .ToList();

                var result = await _settingsService.UpdateBatchAsync(updates);
                if (result.IsSuccess)
                {
                    StatusMessage = $"تم حفظ {updates.Count} إعداد بنجاح";
                }
                else
                {
                    ErrorMessage = result.ErrorMessage;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = FriendlyErrorMessage("الحفظ", ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        // ── Group Filter ────────────────────────────────────────

        private void ApplyGroupFilter()
        {
            FilteredSettings.Clear();
            var source = _selectedGroup == "الكل"
                ? AllSettings
                : AllSettings.Where(s => s.GroupName == _selectedGroup);

            foreach (var s in source)
                FilteredSettings.Add(s);
        }
    }
}

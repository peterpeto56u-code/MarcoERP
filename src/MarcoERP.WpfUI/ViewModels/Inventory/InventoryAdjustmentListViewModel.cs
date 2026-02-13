using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using MarcoERP.Application.DTOs.Inventory;
using MarcoERP.Application.Interfaces.Inventory;
using MarcoERP.WpfUI.Navigation;

namespace MarcoERP.WpfUI.ViewModels.Inventory
{
    /// <summary>
    /// ViewModel for Inventory Adjustment list screen (تسويات المخزون).
    /// </summary>
    public sealed class InventoryAdjustmentListViewModel : BaseViewModel, INavigationAware
    {
        private readonly IInventoryAdjustmentService _adjustmentService;
        private readonly INavigationService _navigationService;

        public InventoryAdjustmentListViewModel(
            IInventoryAdjustmentService adjustmentService,
            INavigationService navigationService)
        {
            _adjustmentService = adjustmentService ?? throw new ArgumentNullException(nameof(adjustmentService));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

            AllAdjustments = new ObservableCollection<InventoryAdjustmentListDto>();

            LoadCommand = new AsyncRelayCommand(LoadAsync);
            NewCommand = new AsyncRelayCommand(NewAsync);
            EditCommand = new RelayCommand(_ => EditSelected());
            JumpToAdjustmentCommand = new AsyncRelayCommand(JumpToAdjustmentAsync);
        }

        // ── Collections ──────────────────────────────────────────

        public ObservableCollection<InventoryAdjustmentListDto> AllAdjustments { get; }

        private Dictionary<string, int> _adjustmentNumberToId = new(StringComparer.OrdinalIgnoreCase);

        private string _jumpAdjustmentNumber;
        public string JumpAdjustmentNumber
        {
            get => _jumpAdjustmentNumber;
            set => SetProperty(ref _jumpAdjustmentNumber, value);
        }

        // ── Selection ────────────────────────────────────────────

        private InventoryAdjustmentListDto _selectedItem;
        public InventoryAdjustmentListDto SelectedItem
        {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value);
        }

        // ── Commands ─────────────────────────────────────────────

        public ICommand LoadCommand { get; }
        public ICommand NewCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand JumpToAdjustmentCommand { get; }

        // ── Load ─────────────────────────────────────────────────

        public async Task LoadAsync()
        {
            IsBusy = true;
            ClearError();
            try
            {
                var result = await _adjustmentService.GetAllAsync();
                if (result.IsSuccess)
                {
                    AllAdjustments.Clear();
                    var list = result.Data.ToList();
                    foreach (var item in list)
                        AllAdjustments.Add(item);
                    _adjustmentNumberToId = list
                        .Where(a => !string.IsNullOrWhiteSpace(a.AdjustmentNumber))
                        .GroupBy(a => a.AdjustmentNumber)
                        .ToDictionary(g => g.Key, g => g.First().Id, StringComparer.OrdinalIgnoreCase);
                    StatusMessage = $"تم تحميل {AllAdjustments.Count} تسوية";
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

        // ── New ──────────────────────────────────────────────────

        private async Task NewAsync()
        {
            _navigationService.NavigateTo("InventoryAdjustmentDetail");
            await Task.CompletedTask;
        }

        // ── Edit ─────────────────────────────────────────────────

        private void EditSelected()
        {
            if (SelectedItem == null) return;
            _navigationService.NavigateTo("InventoryAdjustmentDetail", SelectedItem.Id);
        }

        private async Task JumpToAdjustmentAsync()
        {
            if (string.IsNullOrWhiteSpace(JumpAdjustmentNumber))
                return;

            if (_adjustmentNumberToId.Count == 0)
                await LoadAsync();

            if (!_adjustmentNumberToId.TryGetValue(JumpAdjustmentNumber.Trim(), out var id))
            {
                ErrorMessage = "رقم التسوية غير موجود.";
                return;
            }

            _navigationService.NavigateTo("InventoryAdjustmentDetail", id);
        }

        // ── INavigationAware ─────────────────────────────────────

        public Task OnNavigatedToAsync(object parameter)
        {
            return LoadAsync();
        }

        public Task<bool> OnNavigatingFromAsync()
        {
            return Task.FromResult(true);
        }
    }
}

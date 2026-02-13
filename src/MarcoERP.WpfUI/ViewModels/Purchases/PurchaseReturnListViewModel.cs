using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using MarcoERP.Application.DTOs.Purchases;
using MarcoERP.Application.Interfaces.Purchases;
using MarcoERP.WpfUI.Common;
using MarcoERP.WpfUI.Navigation;

namespace MarcoERP.WpfUI.ViewModels.Purchases
{
    /// <summary>
    /// Full-screen list ViewModel for Purchase Returns.
    /// </summary>
    public sealed class PurchaseReturnListViewModel : BaseViewModel
    {
        private readonly IPurchaseReturnService _returnService;
        private readonly INavigationService _navigationService;

        public ObservableCollection<PurchaseReturnListDto> Returns { get; } = new();

        private PurchaseReturnListDto _selectedItem;
        public PurchaseReturnListDto SelectedItem
        {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value);
        }

        public ICommand LoadCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand NewCommand { get; }
        public ICommand OpenDetailCommand { get; }

        public PurchaseReturnListViewModel(
            IPurchaseReturnService returnService,
            INavigationService navigationService)
        {
            _returnService = returnService ?? throw new ArgumentNullException(nameof(returnService));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

            LoadCommand = new AsyncRelayCommand(LoadAsync);
            RefreshCommand = new AsyncRelayCommand(LoadAsync);
            NewCommand = new RelayCommand(_ => _navigationService.NavigateTo("PurchaseReturnDetail"));
            OpenDetailCommand = new RelayCommand(_ =>
            {
                if (SelectedItem != null)
                    _navigationService.NavigateTo("PurchaseReturnDetail", SelectedItem.Id);
            });
        }

        private async Task LoadAsync()
        {
            IsBusy = true; ClearError();
            try
            {
                var result = await _returnService.GetAllAsync();
                Returns.Clear();
                if (result.IsSuccess)
                    foreach (var r in result.Data) Returns.Add(r);
                StatusMessage = $"تم تحميل {Returns.Count} مرتجع شراء";
            }
            catch (Exception ex) { ErrorMessage = FriendlyErrorMessage("التحميل", ex); }
            finally { IsBusy = false; }
        }
    }
}

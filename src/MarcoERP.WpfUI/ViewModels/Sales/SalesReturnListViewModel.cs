using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using MarcoERP.Application.DTOs.Sales;
using MarcoERP.Application.Interfaces.Sales;
using MarcoERP.WpfUI.Common;
using MarcoERP.WpfUI.Navigation;

namespace MarcoERP.WpfUI.ViewModels.Sales
{
    /// <summary>
    /// Full-screen list ViewModel for Sales Returns.
    /// </summary>
    public sealed class SalesReturnListViewModel : BaseViewModel
    {
        private readonly ISalesReturnService _returnService;
        private readonly INavigationService _navigationService;

        public ObservableCollection<SalesReturnListDto> Returns { get; } = new();

        private SalesReturnListDto _selectedItem;
        public SalesReturnListDto SelectedItem
        {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value);
        }

        public ICommand LoadCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand NewCommand { get; }
        public ICommand OpenDetailCommand { get; }

        public SalesReturnListViewModel(
            ISalesReturnService returnService,
            INavigationService navigationService)
        {
            _returnService = returnService ?? throw new ArgumentNullException(nameof(returnService));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

            LoadCommand = new AsyncRelayCommand(LoadAsync);
            RefreshCommand = new AsyncRelayCommand(LoadAsync);
            NewCommand = new RelayCommand(_ => _navigationService.NavigateTo("SalesReturnDetail"));
            OpenDetailCommand = new RelayCommand(_ =>
            {
                if (SelectedItem != null)
                    _navigationService.NavigateTo("SalesReturnDetail", SelectedItem.Id);
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
                StatusMessage = $"تم تحميل {Returns.Count} مرتجع بيع";
            }
            catch (Exception ex) { ErrorMessage = FriendlyErrorMessage("التحميل", ex); }
            finally { IsBusy = false; }
        }
    }
}

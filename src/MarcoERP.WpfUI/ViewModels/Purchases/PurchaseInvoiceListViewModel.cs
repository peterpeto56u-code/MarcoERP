using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using MarcoERP.Application.DTOs.Purchases;
using MarcoERP.Application.Interfaces.Purchases;
using MarcoERP.WpfUI.Common;
using MarcoERP.WpfUI.Navigation;

namespace MarcoERP.WpfUI.ViewModels.Purchases
{
    /// <summary>
    /// ViewModel for the Purchase Invoice list screen.
    /// </summary>
    public sealed class PurchaseInvoiceListViewModel : BaseViewModel
    {
        private readonly IPurchaseInvoiceService _invoiceService;
        private readonly INavigationService _navigationService;

        public ObservableCollection<PurchaseInvoiceListDto> Invoices { get; } = new();

        private PurchaseInvoiceListDto _selectedItem;
        public PurchaseInvoiceListDto SelectedItem
        {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value);
        }

        public ICommand LoadCommand { get; }
        public ICommand NewCommand { get; }
        public ICommand OpenDetailCommand { get; }
        public ICommand RefreshCommand { get; }

        public PurchaseInvoiceListViewModel(
            IPurchaseInvoiceService invoiceService,
            INavigationService navigationService)
        {
            _invoiceService = invoiceService ?? throw new ArgumentNullException(nameof(invoiceService));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

            LoadCommand = new AsyncRelayCommand(LoadInvoicesAsync);
            NewCommand = new RelayCommand(_ => _navigationService.NavigateTo("PurchaseInvoiceDetail"));
            OpenDetailCommand = new RelayCommand(_ => { if (SelectedItem != null) _navigationService.NavigateTo("PurchaseInvoiceDetail", SelectedItem.Id); });
            RefreshCommand = new AsyncRelayCommand(LoadInvoicesAsync);
        }

        public async Task LoadInvoicesAsync()
        {
            IsBusy = true;
            ClearError();
            try
            {
                var result = await _invoiceService.GetAllAsync();
                Invoices.Clear();
                if (result.IsSuccess)
                    foreach (var inv in result.Data)
                        Invoices.Add(inv);
                StatusMessage = $"تم تحميل {Invoices.Count} فاتورة شراء";
            }
            catch (Exception ex) { ErrorMessage = FriendlyErrorMessage("التحميل", ex); }
            finally { IsBusy = false; }
        }
    }
}

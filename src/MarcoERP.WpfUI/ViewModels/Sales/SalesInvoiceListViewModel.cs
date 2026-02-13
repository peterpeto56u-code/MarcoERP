using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using MarcoERP.Application.DTOs.Sales;
using MarcoERP.Application.Interfaces.Sales;
using MarcoERP.WpfUI.Common;
using MarcoERP.WpfUI.Navigation;

namespace MarcoERP.WpfUI.ViewModels.Sales
{
    /// <summary>
    /// ViewModel for the Sales Invoice list screen.
    /// Displays all invoices in a grid with navigation to detail view.
    /// </summary>
    public sealed class SalesInvoiceListViewModel : BaseViewModel
    {
        private readonly ISalesInvoiceService _invoiceService;
        private readonly INavigationService _navigationService;

        public ObservableCollection<SalesInvoiceListDto> Invoices { get; } = new();

        private SalesInvoiceListDto _selectedItem;
        public SalesInvoiceListDto SelectedItem
        {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value);
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                    ApplyFilter();
            }
        }

        public ICommand LoadCommand { get; }
        public ICommand NewCommand { get; }
        public ICommand OpenDetailCommand { get; }
        public ICommand RefreshCommand { get; }

        public SalesInvoiceListViewModel(
            ISalesInvoiceService invoiceService,
            INavigationService navigationService)
        {
            _invoiceService = invoiceService ?? throw new ArgumentNullException(nameof(invoiceService));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

            LoadCommand = new AsyncRelayCommand(LoadInvoicesAsync);
            NewCommand = new RelayCommand(_ => NavigateToNew());
            OpenDetailCommand = new RelayCommand(_ => NavigateToDetail());
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

                StatusMessage = $"تم تحميل {Invoices.Count} فاتورة بيع";
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

        private void NavigateToNew()
        {
            _navigationService.NavigateTo("SalesInvoiceDetail");
        }

        private void NavigateToDetail()
        {
            if (SelectedItem == null) return;
            _navigationService.NavigateTo("SalesInvoiceDetail", SelectedItem.Id);
        }

        private void ApplyFilter()
        {
            // Filter is handled client-side on the existing collection
            // A more sophisticated implementation could use CollectionViewSource
        }
    }
}

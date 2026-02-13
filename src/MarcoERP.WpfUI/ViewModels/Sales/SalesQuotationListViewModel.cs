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
    /// ViewModel for the Sales Quotation list screen.
    /// Displays all quotations in a grid with navigation to detail view.
    /// </summary>
    public sealed class SalesQuotationListViewModel : BaseViewModel
    {
        private readonly ISalesQuotationService _quotationService;
        private readonly INavigationService _navigationService;

        public ObservableCollection<SalesQuotationListDto> Quotations { get; } = new();

        private SalesQuotationListDto _selectedItem;
        public SalesQuotationListDto SelectedItem
        {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value);
        }

        public ICommand LoadCommand { get; }
        public ICommand NewCommand { get; }
        public ICommand OpenDetailCommand { get; }
        public ICommand RefreshCommand { get; }

        public SalesQuotationListViewModel(
            ISalesQuotationService quotationService,
            INavigationService navigationService)
        {
            _quotationService = quotationService ?? throw new ArgumentNullException(nameof(quotationService));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

            LoadCommand = new AsyncRelayCommand(LoadQuotationsAsync);
            NewCommand = new RelayCommand(_ => NavigateToNew());
            OpenDetailCommand = new RelayCommand(_ => NavigateToDetail());
            RefreshCommand = new AsyncRelayCommand(LoadQuotationsAsync);
        }

        public async Task LoadQuotationsAsync()
        {
            IsBusy = true;
            ClearError();
            try
            {
                var result = await _quotationService.GetAllAsync();
                Quotations.Clear();
                if (result.IsSuccess)
                    foreach (var q in result.Data)
                        Quotations.Add(q);

                StatusMessage = $"تم تحميل {Quotations.Count} عرض سعر";
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
            _navigationService.NavigateTo("SalesQuotationDetail");
        }

        private void NavigateToDetail()
        {
            if (SelectedItem == null) return;
            _navigationService.NavigateTo("SalesQuotationDetail", SelectedItem.Id);
        }
    }
}

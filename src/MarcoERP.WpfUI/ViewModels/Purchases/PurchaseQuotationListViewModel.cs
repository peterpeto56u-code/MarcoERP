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
    /// ViewModel for the Purchase Quotation list screen.
    /// </summary>
    public sealed class PurchaseQuotationListViewModel : BaseViewModel
    {
        private readonly IPurchaseQuotationService _quotationService;
        private readonly INavigationService _navigationService;

        public ObservableCollection<PurchaseQuotationListDto> Quotations { get; } = new();

        private PurchaseQuotationListDto _selectedItem;
        public PurchaseQuotationListDto SelectedItem
        {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value);
        }

        public ICommand LoadCommand { get; }
        public ICommand NewCommand { get; }
        public ICommand OpenDetailCommand { get; }
        public ICommand RefreshCommand { get; }

        public PurchaseQuotationListViewModel(
            IPurchaseQuotationService quotationService,
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

                StatusMessage = $"تم تحميل {Quotations.Count} طلب شراء";
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
            _navigationService.NavigateTo("PurchaseQuotationDetail");
        }

        private void NavigateToDetail()
        {
            if (SelectedItem == null) return;
            _navigationService.NavigateTo("PurchaseQuotationDetail", SelectedItem.Id);
        }
    }
}

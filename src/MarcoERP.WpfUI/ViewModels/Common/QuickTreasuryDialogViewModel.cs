using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using MarcoERP.Application.DTOs.Treasury;
using MarcoERP.Application.Interfaces.Treasury;
using MarcoERP.Domain.Enums;
using MarcoERP.WpfUI.Common;
using MarcoERP.WpfUI.Services;

namespace MarcoERP.WpfUI.ViewModels.Common
{
    public sealed class QuickTreasuryDialogViewModel : BaseViewModel
    {
        private readonly ICashboxService _cashboxService;
        private QuickTreasuryDialogRequest _request;

        public ObservableCollection<CashboxDto> Cashboxes { get; } = new();
        public ObservableCollection<PaymentMethodOption> PaymentMethods { get; } = new();

        private int? _selectedCashboxId;
        public int? SelectedCashboxId
        {
            get => _selectedCashboxId;
            set { SetProperty(ref _selectedCashboxId, value); OnPropertyChanged(nameof(CanConfirm)); }
        }

        private PaymentMethodOption _selectedPaymentMethod;
        public PaymentMethodOption SelectedPaymentMethod
        {
            get => _selectedPaymentMethod;
            set { SetProperty(ref _selectedPaymentMethod, value); OnPropertyChanged(nameof(CanConfirm)); }
        }

        private decimal _amount;
        public decimal Amount
        {
            get => _amount;
            set { SetProperty(ref _amount, value); OnPropertyChanged(nameof(CanConfirm)); }
        }

        public string TitleText => _request?.Title ?? "";
        public string Description => _request?.Description ?? "";
        public string Notes => _request?.Notes ?? "";

        public bool CanConfirm => SelectedCashboxId > 0 && Amount > 0 && SelectedPaymentMethod != null;

        public ICommand LoadedCommand { get; }
        public ICommand ConfirmCommand { get; }
        public ICommand CancelCommand { get; }

        public event Action<bool> RequestClose;

        public QuickTreasuryDialogResult Result { get; private set; }

        public QuickTreasuryDialogViewModel(ICashboxService cashboxService)
        {
            _cashboxService = cashboxService ?? throw new ArgumentNullException(nameof(cashboxService));

            LoadedCommand = new AsyncRelayCommand(LoadAsync);
            ConfirmCommand = new RelayCommand(_ => Confirm(), _ => CanConfirm);
            CancelCommand = new RelayCommand(_ => Cancel());

            SeedPaymentMethods();
        }

        public void Initialize(QuickTreasuryDialogRequest request)
        {
            _request = request;
            OnPropertyChanged(nameof(TitleText));
            OnPropertyChanged(nameof(Description));
            OnPropertyChanged(nameof(Notes));

            Amount = request.DefaultAmount;
        }

        private void SeedPaymentMethods()
        {
            PaymentMethods.Clear();
            PaymentMethods.Add(new PaymentMethodOption(PaymentMethod.Cash, "نقدي"));
            PaymentMethods.Add(new PaymentMethodOption(PaymentMethod.Card, "بطاقة"));
            SelectedPaymentMethod = PaymentMethods.FirstOrDefault();
        }

        private async Task LoadAsync()
        {
            ClearError();
            try
            {
                var result = await _cashboxService.GetAllAsync();
                Cashboxes.Clear();
                if (result.IsSuccess)
                {
                    foreach (var c in result.Data.Where(x => x.IsActive))
                        Cashboxes.Add(c);
                }

                if (SelectedCashboxId == null)
                {
                    var def = Cashboxes.FirstOrDefault(x => x.IsDefault) ?? Cashboxes.FirstOrDefault();
                    if (def != null) SelectedCashboxId = def.Id;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = FriendlyErrorMessage("تحميل الخزن", ex);
            }
        }

        private void Confirm()
        {
            if (!CanConfirm) return;

            Result = new QuickTreasuryDialogResult(
                SelectedCashboxId!.Value,
                SelectedPaymentMethod.Value,
                Amount);

            RequestClose?.Invoke(true);
        }

        private void Cancel()
        {
            RequestClose?.Invoke(false);
        }

        public sealed record PaymentMethodOption(PaymentMethod Value, string DisplayName)
        {
            public override string ToString() => DisplayName;
        }
    }
}

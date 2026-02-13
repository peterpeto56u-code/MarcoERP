using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MarcoERP.Application.DTOs.Purchases;
using MarcoERP.Application.Interfaces.Purchases;
using MarcoERP.Domain.Exceptions;
using MarcoERP.WpfUI.Common;

namespace MarcoERP.WpfUI.ViewModels.Purchases
{
    /// <summary>
    /// ViewModel for Supplier management screen.
    /// </summary>
    public sealed class SupplierViewModel : BaseViewModel
    {
        private readonly ISupplierService _supplierService;

        public SupplierViewModel(ISupplierService supplierService)
        {
            _supplierService = supplierService ?? throw new ArgumentNullException(nameof(supplierService));

            AllSuppliers = new ObservableCollection<SupplierDto>();

            LoadCommand = new AsyncRelayCommand(LoadSuppliersAsync);
            NewCommand = new AsyncRelayCommand(PrepareNewAsync);
            SaveCommand = new AsyncRelayCommand(SaveAsync, () => CanSave);
            DeleteCommand = new AsyncRelayCommand(DeleteAsync, () => SelectedItem != null);
            DeactivateCommand = new AsyncRelayCommand(DeactivateAsync, () => CanDeactivate);
            ActivateCommand = new AsyncRelayCommand(ActivateAsync, () => CanActivate);
            CancelCommand = new RelayCommand(CancelEditing);
            EditSelectedCommand = new RelayCommand(_ => EditSelected());
        }

        // ── Collections ──────────────────────────────────────────

        public ObservableCollection<SupplierDto> AllSuppliers { get; }

        // ── Selection ────────────────────────────────────────────

        private SupplierDto _selectedItem;
        public SupplierDto SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (SetProperty(ref _selectedItem, value))
                {
                    if (value != null && !IsEditing)
                        PopulateForm(value);
                    OnPropertyChanged(nameof(CanDeactivate));
                    OnPropertyChanged(nameof(CanActivate));
                }
            }
        }

        // ── Form Fields ─────────────────────────────────────────

        private bool _isEditing;
        public bool IsEditing
        {
            get => _isEditing;
            set => SetProperty(ref _isEditing, value);
        }

        private bool _isNew;
        public bool IsNew
        {
            get => _isNew;
            set => SetProperty(ref _isNew, value);
        }

        private string _formCode;
        public string FormCode
        {
            get => _formCode;
            set => SetProperty(ref _formCode, value);
        }

        private string _formNameAr;
        public string FormNameAr
        {
            get => _formNameAr;
            set { SetProperty(ref _formNameAr, value); OnPropertyChanged(nameof(CanSave)); }
        }

        private string _formNameEn;
        public string FormNameEn
        {
            get => _formNameEn;
            set => SetProperty(ref _formNameEn, value);
        }

        private string _formPhone;
        public string FormPhone
        {
            get => _formPhone;
            set => SetProperty(ref _formPhone, value);
        }

        private string _formMobile;
        public string FormMobile
        {
            get => _formMobile;
            set => SetProperty(ref _formMobile, value);
        }

        private string _formAddress;
        public string FormAddress
        {
            get => _formAddress;
            set => SetProperty(ref _formAddress, value);
        }

        private string _formCity;
        public string FormCity
        {
            get => _formCity;
            set => SetProperty(ref _formCity, value);
        }

        private string _formTaxNumber;
        public string FormTaxNumber
        {
            get => _formTaxNumber;
            set => SetProperty(ref _formTaxNumber, value);
        }

        private decimal _formPreviousBalance;
        public decimal FormPreviousBalance
        {
            get => _formPreviousBalance;
            set => SetProperty(ref _formPreviousBalance, value);
        }

        private string _formNotes;
        public string FormNotes
        {
            get => _formNotes;
            set => SetProperty(ref _formNotes, value);
        }

        // ── Commands ─────────────────────────────────────────────

        public ICommand LoadCommand { get; }
        public ICommand NewCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand DeactivateCommand { get; }
        public ICommand ActivateCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand EditSelectedCommand { get; }

        // ── Can Execute ──────────────────────────────────────────

        public bool CanSave => !string.IsNullOrWhiteSpace(FormNameAr);
        public bool CanDeactivate => SelectedItem != null && SelectedItem.IsActive;
        public bool CanActivate => SelectedItem != null && !SelectedItem.IsActive;

        // ── Load ─────────────────────────────────────────────────

        public async Task LoadSuppliersAsync()
        {
            IsBusy = true;
            ClearError();
            try
            {
                var result = await _supplierService.GetAllAsync();
                if (result.IsSuccess)
                {
                    AllSuppliers.Clear();
                    foreach (var s in result.Data)
                        AllSuppliers.Add(s);
                    StatusMessage = $"تم تحميل {AllSuppliers.Count} مورد";
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

        private async Task PrepareNewAsync()
        {
            IsEditing = true;
            IsNew = true;
            ClearError();

            try
            {
                var codeResult = await _supplierService.GetNextCodeAsync();
                FormCode = codeResult.IsSuccess ? codeResult.Data : "";
            }
            catch
            {
                FormCode = "";
            }

            FormNameAr = "";
            FormNameEn = "";
            FormPhone = "";
            FormMobile = "";
            FormAddress = "";
            FormCity = "";
            FormTaxNumber = "";
            FormPreviousBalance = 0;
            FormNotes = "";
            StatusMessage = "إدخال مورد جديد...";
        }

        // ── Save ─────────────────────────────────────────────────

        private async Task SaveAsync()
        {
            IsBusy = true;
            ClearError();
            try
            {
                if (IsNew)
                {
                    var dto = new CreateSupplierDto
                    {
                        Code = FormCode,
                        NameAr = FormNameAr,
                        NameEn = FormNameEn,
                        Phone = FormPhone,
                        Mobile = FormMobile,
                        Address = FormAddress,
                        City = FormCity,
                        TaxNumber = FormTaxNumber,
                        PreviousBalance = FormPreviousBalance,
                        Notes = FormNotes
                    };
                    var result = await _supplierService.CreateAsync(dto);
                    if (result.IsSuccess)
                    {
                        StatusMessage = $"تم إنشاء المورد: {result.Data.Code} — {result.Data.NameAr}";
                        IsEditing = false;
                        IsNew = false;
                        await LoadSuppliersAsync();
                    }
                    else
                    {
                        ErrorMessage = result.ErrorMessage;
                    }
                }
                else
                {
                    var dto = new UpdateSupplierDto
                    {
                        Id = SelectedItem.Id,
                        NameAr = FormNameAr,
                        NameEn = FormNameEn,
                        Phone = FormPhone,
                        Mobile = FormMobile,
                        Address = FormAddress,
                        City = FormCity,
                        TaxNumber = FormTaxNumber,
                        Notes = FormNotes
                    };
                    var result = await _supplierService.UpdateAsync(dto);
                    if (result.IsSuccess)
                    {
                        StatusMessage = $"تم تحديث المورد: {result.Data.NameAr}";
                        IsEditing = false;
                        await LoadSuppliersAsync();
                    }
                    else
                    {
                        ErrorMessage = result.ErrorMessage;
                    }
                }
            }
            catch (ConcurrencyConflictException ex)
            {
                await ConcurrencyHelper.ShowConflictAndRefreshAsync(ex, LoadSuppliersAsync);
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

        // ── Delete ──────────────────────────────────────────────

        private async Task DeleteAsync()
        {
            if (SelectedItem == null) return;

            var confirm = MessageBox.Show(
                $"هل أنت متأكد من حذف المورد «{SelectedItem.NameAr}»؟\nالحذف سيكون ناعم (Soft Delete).",
                "تأكيد الحذف",
                MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
            if (confirm != MessageBoxResult.Yes) return;

            IsBusy = true;
            ClearError();
            try
            {
                var result = await _supplierService.DeleteAsync(SelectedItem.Id);
                if (result.IsSuccess)
                {
                    StatusMessage = "تم حذف المورد بنجاح";
                    await LoadSuppliersAsync();
                }
                else
                {
                    ErrorMessage = result.ErrorMessage;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = FriendlyErrorMessage("الحذف", ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        // ── Deactivate ──────────────────────────────────────────

        private async Task DeactivateAsync()
        {
            if (SelectedItem == null) return;

            IsBusy = true;
            ClearError();
            try
            {
                var result = await _supplierService.DeactivateAsync(SelectedItem.Id);
                if (result.IsSuccess)
                {
                    StatusMessage = "تم تعطيل المورد";
                    await LoadSuppliersAsync();
                }
                else
                {
                    ErrorMessage = result.ErrorMessage;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = FriendlyErrorMessage("التعطيل", ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        // ── Activate ────────────────────────────────────────────

        private async Task ActivateAsync()
        {
            if (SelectedItem == null) return;

            IsBusy = true;
            ClearError();
            try
            {
                var result = await _supplierService.ActivateAsync(SelectedItem.Id);
                if (result.IsSuccess)
                {
                    StatusMessage = "تم تفعيل المورد";
                    await LoadSuppliersAsync();
                }
                else
                {
                    ErrorMessage = result.ErrorMessage;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = FriendlyErrorMessage("التفعيل", ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        // ── Cancel ──────────────────────────────────────────────

        private void CancelEditing(object parameter)
        {
            IsEditing = false;
            IsNew = false;
            ClearError();
            StatusMessage = "تم الإلغاء";
            if (SelectedItem != null) PopulateForm(SelectedItem);
        }

        // ── Helpers ─────────────────────────────────────────────

        private void PopulateForm(SupplierDto item)
        {
            FormCode = item.Code;
            FormNameAr = item.NameAr;
            FormNameEn = item.NameEn;
            FormPhone = item.Phone;
            FormMobile = item.Mobile;
            FormAddress = item.Address;
            FormCity = item.City;
            FormTaxNumber = item.TaxNumber;
            FormPreviousBalance = item.PreviousBalance;
            FormNotes = item.Notes;
            IsEditing = false;
            IsNew = false;
        }

        public void EditSelected()
        {
            if (SelectedItem == null) return;
            IsEditing = true;
            IsNew = false;
            PopulateForm(SelectedItem);
            IsEditing = true;
        }
    }
}

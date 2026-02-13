using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MarcoERP.Application.Common;
using MarcoERP.Application.DTOs.Accounting;
using MarcoERP.Application.Interfaces.Accounting;
using MarcoERP.Domain.Enums;
using MarcoERP.Domain.Exceptions;
using MarcoERP.WpfUI.Common;

namespace MarcoERP.WpfUI.ViewModels.Accounting
{
    /// <summary>
    /// ViewModel for Chart of Accounts management screen.
    /// Displays hierarchical tree + form for CRUD.
    /// </summary>
    public sealed class ChartOfAccountsViewModel : BaseViewModel
    {
        private readonly IAccountService _accountService;

        public ChartOfAccountsViewModel(IAccountService accountService)
        {
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));

            AccountTree = new ObservableCollection<AccountTreeNodeDto>();
            AllAccounts = new ObservableCollection<AccountDto>();

            // Commands
            LoadCommand = new AsyncRelayCommand(LoadAccountsAsync);
            NewAccountCommand = new RelayCommand(PrepareNewAccount);
            EditSelectedAccountCommand = new RelayCommand(EditSelectedAccount, () => SelectedAccount != null && !SelectedAccount.IsSystemAccount);
            SaveCommand = new AsyncRelayCommand(SaveAccountAsync, () => CanSave);
            DeleteCommand = new AsyncRelayCommand(DeleteAccountAsync, () => CanDelete);
            DeactivateCommand = new AsyncRelayCommand(DeactivateAccountAsync, () => CanDeactivate);
            ActivateCommand = new AsyncRelayCommand(ActivateAccountAsync, () => CanActivate);
            CancelCommand = new RelayCommand(CancelEditing);
        }

        // ── Collections ──────────────────────────────────────────

        public ObservableCollection<AccountTreeNodeDto> AccountTree { get; }
        public ObservableCollection<AccountDto> AllAccounts { get; }

        // ── Selection ────────────────────────────────────────────

        private AccountDto _selectedAccount;
        public AccountDto SelectedAccount
        {
            get => _selectedAccount;
            set
            {
                if (SetProperty(ref _selectedAccount, value))
                {
                    if (value != null && !IsEditing)
                    {
                        PopulateFormFromAccount(value);
                    }
                    OnPropertyChanged(nameof(CanDelete));
                    OnPropertyChanged(nameof(CanDeactivate));
                    OnPropertyChanged(nameof(CanActivate));
                    RelayCommand.RaiseCanExecuteChanged();
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

        private bool _isNewAccount;
        public bool IsNewAccount
        {
            get => _isNewAccount;
            set => SetProperty(ref _isNewAccount, value);
        }

        private string _formAccountCode;
        public string FormAccountCode
        {
            get => _formAccountCode;
            set { SetProperty(ref _formAccountCode, value); OnPropertyChanged(nameof(CanSave)); }
        }

        private string _formAccountNameAr;
        public string FormAccountNameAr
        {
            get => _formAccountNameAr;
            set { SetProperty(ref _formAccountNameAr, value); OnPropertyChanged(nameof(CanSave)); }
        }

        private string _formAccountNameEn;
        public string FormAccountNameEn
        {
            get => _formAccountNameEn;
            set => SetProperty(ref _formAccountNameEn, value);
        }

        private AccountType _formAccountType;
        public AccountType FormAccountType
        {
            get => _formAccountType;
            set => SetProperty(ref _formAccountType, value);
        }

        private int _formLevel;
        public int FormLevel
        {
            get => _formLevel;
            set => SetProperty(ref _formLevel, value);
        }

        private int? _formParentAccountId;
        public int? FormParentAccountId
        {
            get => _formParentAccountId;
            set => SetProperty(ref _formParentAccountId, value);
        }

        private string _formDescription;
        public string FormDescription
        {
            get => _formDescription;
            set => SetProperty(ref _formDescription, value);
        }

        private string _formCurrencyCode = "EGP";
        public string FormCurrencyCode
        {
            get => _formCurrencyCode;
            set => SetProperty(ref _formCurrencyCode, value);
        }

        private byte[] _formRowVersion;

        // ── Account Types for ComboBox ──
        public IReadOnlyList<AccountType> AccountTypes { get; } = new List<AccountType>
        {
            AccountType.Asset,
            AccountType.Liability,
            AccountType.Equity,
            AccountType.Revenue,
            AccountType.COGS,
            AccountType.Expense,
            AccountType.OtherIncome,
            AccountType.OtherExpense
        };

        public IReadOnlyList<int> Levels { get; } = new List<int> { 1, 2, 3, 4 };

        // ── Parent account list for combo ──
        private ObservableCollection<AccountDto> _parentAccounts = new ObservableCollection<AccountDto>();
        public ObservableCollection<AccountDto> ParentAccounts
        {
            get => _parentAccounts;
            set => SetProperty(ref _parentAccounts, value);
        }

        // ── Commands ─────────────────────────────────────────────

        public ICommand LoadCommand { get; }
        public ICommand NewAccountCommand { get; }
        public ICommand EditSelectedAccountCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand DeactivateCommand { get; }
        public ICommand ActivateCommand { get; }
        public ICommand CancelCommand { get; }

        // ── Can Execute Logic ────────────────────────────────────

        public bool CanSave => !string.IsNullOrWhiteSpace(FormAccountCode)
                             && !string.IsNullOrWhiteSpace(FormAccountNameAr);

        public bool CanDelete => SelectedAccount != null
                               && !SelectedAccount.IsSystemAccount
                               && !SelectedAccount.HasPostings;

        public bool CanDeactivate => SelectedAccount != null && SelectedAccount.IsActive;

        public bool CanActivate => SelectedAccount != null && !SelectedAccount.IsActive;

        // ── Load ─────────────────────────────────────────────────

        public async Task LoadAccountsAsync()
        {
            IsBusy = true;
            ClearError();
            try
            {
                var treeResult = await _accountService.GetAccountTreeAsync();
                var allResult = await _accountService.GetAllAsync();

                if (treeResult.IsSuccess)
                {
                    AccountTree.Clear();
                    foreach (var node in treeResult.Data)
                        AccountTree.Add(node);
                }

                if (allResult.IsSuccess)
                {
                    AllAccounts.Clear();
                    foreach (var acc in allResult.Data)
                        AllAccounts.Add(acc);

                    // Build parent list (non-leaf accounts that can have children)
                    ParentAccounts.Clear();
                    foreach (var acc in allResult.Data.Where(a => a.Level < 4))
                        ParentAccounts.Add(acc);
                }

                StatusMessage = $"تم تحميل {AllAccounts.Count} حساب";
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

        // ── New Account ──────────────────────────────────────────

        private void PrepareNewAccount(object parameter)
        {
            IsEditing = true;
            IsNewAccount = true;
            ClearError();

            // If a parent is selected, derive defaults
            if (SelectedAccount != null)
            {
                FormParentAccountId = SelectedAccount.Id;
                FormAccountType = SelectedAccount.AccountType;
                FormLevel = Math.Min(SelectedAccount.Level + 1, 4);
                FormAccountCode = "";
            }
            else
            {
                FormAccountCode = "";
                FormLevel = 1;
                FormParentAccountId = null;
                FormAccountType = AccountType.Asset;
            }

            FormAccountNameAr = "";
            FormAccountNameEn = "";
            FormDescription = "";
            FormCurrencyCode = "EGP";
            _formRowVersion = null;

            StatusMessage = "إدخال حساب جديد...";
        }

        // ── Save ─────────────────────────────────────────────────

        private async Task SaveAccountAsync()
        {
            IsBusy = true;
            ClearError();
            try
            {
                if (IsNewAccount)
                {
                    var dto = new CreateAccountDto
                    {
                        AccountCode = FormAccountCode,
                        AccountNameAr = FormAccountNameAr,
                        AccountNameEn = FormAccountNameEn,
                        AccountType = FormAccountType,
                        Level = FormLevel,
                        ParentAccountId = FormParentAccountId,
                        Description = FormDescription,
                        CurrencyCode = FormCurrencyCode
                    };

                    var result = await _accountService.CreateAsync(dto);
                    if (result.IsSuccess)
                    {
                        StatusMessage = $"تم إنشاء الحساب: {result.Data.AccountCode} — {result.Data.AccountNameAr}";
                        IsEditing = false;
                        IsNewAccount = false;
                        await LoadAccountsAsync();
                    }
                    else
                    {
                        ErrorMessage = result.ErrorMessage;
                    }
                }
                else
                {
                    // Update existing
                    var dto = new UpdateAccountDto
                    {
                        Id = SelectedAccount.Id,
                        AccountNameAr = FormAccountNameAr,
                        AccountNameEn = FormAccountNameEn,
                        Description = FormDescription,
                        RowVersion = _formRowVersion
                    };

                    var result = await _accountService.UpdateAsync(dto);
                    if (result.IsSuccess)
                    {
                        StatusMessage = $"تم تحديث الحساب: {result.Data.AccountCode}";
                        IsEditing = false;
                        await LoadAccountsAsync();
                    }
                    else
                    {
                        ErrorMessage = result.ErrorMessage;
                    }
                }
            }
            catch (ConcurrencyConflictException ex)
            {
                await ConcurrencyHelper.ShowConflictAndRefreshAsync(ex, LoadAccountsAsync);
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

        // ── Delete ───────────────────────────────────────────────

        private async Task DeleteAccountAsync()
        {
            if (SelectedAccount == null) return;

            var confirm = MessageBox.Show(
                $"هل أنت متأكد من حذف الحساب «{SelectedAccount.AccountNameAr}»؟\nالحذف سيكون ناعم (Soft Delete).",
                "تأكيد الحذف",
                MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);

            if (confirm != MessageBoxResult.Yes) return;

            IsBusy = true;
            ClearError();
            try
            {
                var result = await _accountService.DeleteAsync(SelectedAccount.Id);
                if (result.IsSuccess)
                {
                    StatusMessage = "تم حذف الحساب بنجاح";
                    await LoadAccountsAsync();
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

        private async Task DeactivateAccountAsync()
        {
            if (SelectedAccount == null) return;

            IsBusy = true;
            ClearError();
            try
            {
                var result = await _accountService.DeactivateAsync(SelectedAccount.Id);
                if (result.IsSuccess)
                {
                    StatusMessage = "تم تعطيل الحساب";
                    await LoadAccountsAsync();
                }
                else
                {
                    ErrorMessage = result.ErrorMessage;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = FriendlyErrorMessage("العملية", ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        // ── Activate ────────────────────────────────────────────

        private async Task ActivateAccountAsync()
        {
            if (SelectedAccount == null) return;

            IsBusy = true;
            ClearError();
            try
            {
                var result = await _accountService.ActivateAsync(SelectedAccount.Id);
                if (result.IsSuccess)
                {
                    StatusMessage = "تم تفعيل الحساب";
                    await LoadAccountsAsync();
                }
                else
                {
                    ErrorMessage = result.ErrorMessage;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = FriendlyErrorMessage("العملية", ex);
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
            IsNewAccount = false;
            ClearError();
            StatusMessage = "تم الإلغاء";

            if (SelectedAccount != null)
                PopulateFormFromAccount(SelectedAccount);
        }

        // ── Helpers ─────────────────────────────────────────────

        private void PopulateFormFromAccount(AccountDto account)
        {
            FormAccountCode = account.AccountCode;
            FormAccountNameAr = account.AccountNameAr;
            FormAccountNameEn = account.AccountNameEn;
            FormAccountType = account.AccountType;
            FormLevel = account.Level;
            FormParentAccountId = account.ParentAccountId;
            FormDescription = account.Description;
            FormCurrencyCode = account.CurrencyCode;
            _formRowVersion = account.RowVersion;
            IsEditing = false;
            IsNewAccount = false;
        }

        /// <summary>Starts editing the selected account.</summary>
        public void EditSelectedAccount()
        {
            if (SelectedAccount == null || SelectedAccount.IsSystemAccount) return;
            IsEditing = true;
            IsNewAccount = false;
            PopulateFormFromAccount(SelectedAccount);
            IsEditing = true; // re-set after populate resets it
        }
    }
}

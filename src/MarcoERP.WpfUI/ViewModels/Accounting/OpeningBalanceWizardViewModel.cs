using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using MarcoERP.Application.DTOs.Accounting;
using MarcoERP.Application.Interfaces.Accounting;
using MarcoERP.Domain.Enums;

namespace MarcoERP.WpfUI.ViewModels.Accounting
{
    /// <summary>
    /// ViewModel for the Opening Balance Wizard (D.4).
    /// 3-step workflow: Select Fiscal Year → Enter Balances → Review &amp; Post.
    /// </summary>
    public sealed class OpeningBalanceWizardViewModel : BaseViewModel
    {
        private readonly IJournalEntryService _journalService;
        private readonly IAccountService _accountService;
        private readonly IFiscalYearService _fiscalYearService;

        public OpeningBalanceWizardViewModel(
            IJournalEntryService journalService,
            IAccountService accountService,
            IFiscalYearService fiscalYearService)
        {
            _journalService = journalService ?? throw new ArgumentNullException(nameof(journalService));
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
            _fiscalYearService = fiscalYearService ?? throw new ArgumentNullException(nameof(fiscalYearService));

            FiscalYears = new ObservableCollection<FiscalYearDto>();
            AccountBalances = new ObservableCollection<OpeningBalanceLineItem>();

            LoadCommand = new AsyncRelayCommand(LoadFiscalYearsAsync);
            NextCommand = new AsyncRelayCommand(GoNextAsync, CanGoNext);
            BackCommand = new RelayCommand(GoBack, CanGoBack);
            PostCommand = new AsyncRelayCommand(PostOpeningBalanceAsync, CanPost);

            CurrentStep = 1;
        }

        // ── Collections ──────────────────────────────────────────

        public ObservableCollection<FiscalYearDto> FiscalYears { get; }
        public ObservableCollection<OpeningBalanceLineItem> AccountBalances { get; }

        // ── Step ─────────────────────────────────────────────────

        private int _currentStep;
        public int CurrentStep
        {
            get => _currentStep;
            set
            {
                if (SetProperty(ref _currentStep, value))
                {
                    OnPropertyChanged(nameof(IsStep1));
                    OnPropertyChanged(nameof(IsStep2));
                    OnPropertyChanged(nameof(IsStep3));
                    OnPropertyChanged(nameof(Step1Status));
                    OnPropertyChanged(nameof(Step2Status));
                    OnPropertyChanged(nameof(Step3Status));
                }
            }
        }

        public bool IsStep1 => CurrentStep == 1;
        public bool IsStep2 => CurrentStep == 2;
        public bool IsStep3 => CurrentStep == 3;

        public string Step1Status => CurrentStep > 1 ? "Done" : (CurrentStep == 1 ? "Current" : "Pending");
        public string Step2Status => CurrentStep > 2 ? "Done" : (CurrentStep == 2 ? "Current" : "Pending");
        public string Step3Status => CurrentStep == 3 ? "Current" : "Pending";

        // ── Selected Fiscal Year ─────────────────────────────────

        private FiscalYearDto _selectedFiscalYear;
        public FiscalYearDto SelectedFiscalYear
        {
            get => _selectedFiscalYear;
            set => SetProperty(ref _selectedFiscalYear, value);
        }

        // ── Computed Totals ──────────────────────────────────────

        public decimal TotalDebit => AccountBalances.Sum(x => x.DebitAmount);
        public decimal TotalCredit => AccountBalances.Sum(x => x.CreditAmount);
        public bool IsBalanced => TotalDebit == TotalCredit && TotalDebit > 0;
        public decimal Difference => Math.Abs(TotalDebit - TotalCredit);

        // ── Commands ─────────────────────────────────────────────

        public ICommand LoadCommand { get; }
        public ICommand NextCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand PostCommand { get; }

        // ── Initialization ───────────────────────────────────────

        public async Task LoadFiscalYearsAsync()
        {
            IsBusy = true;
            ClearError();
            try
            {
                var result = await _fiscalYearService.GetAllAsync();
                if (result.IsSuccess)
                {
                    FiscalYears.Clear();
                    foreach (var fy in result.Data)
                    {
                        if (fy.Status == FiscalYearStatus.Active || fy.Status == FiscalYearStatus.Setup)
                            FiscalYears.Add(fy);
                    }
                    StatusMessage = $"تم تحميل {FiscalYears.Count} سنة مالية.";
                }
                else
                {
                    ErrorMessage = string.Join("; ", result.Errors);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = FriendlyErrorMessage("تحميل السنوات المالية", ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        // ── Navigation ───────────────────────────────────────────

        private bool CanGoNext()
        {
            if (IsBusy) return false;
            if (CurrentStep == 1) return SelectedFiscalYear != null;
            if (CurrentStep == 2) return AccountBalances.Count > 0;
            return false;
        }

        private async Task GoNextAsync()
        {
            ClearError();
            if (CurrentStep == 1)
            {
                // Moving from Step 1 → Step 2: load postable accounts
                await LoadPostableAccountsAsync();
                if (!HasError)
                    CurrentStep = 2;
            }
            else if (CurrentStep == 2)
            {
                // Moving from Step 2 → Step 3: validate and show summary
                RefreshTotals();
                CurrentStep = 3;
            }
        }

        private bool CanGoBack()
        {
            return !IsBusy && CurrentStep > 1;
        }

        private void GoBack()
        {
            ClearError();
            if (CurrentStep > 1)
                CurrentStep--;
        }

        // ── Load Accounts ────────────────────────────────────────

        private async Task LoadPostableAccountsAsync()
        {
            IsBusy = true;
            try
            {
                var result = await _accountService.GetPostableAccountsAsync();
                if (result.IsSuccess)
                {
                    foreach (var old in AccountBalances)
                        old.PropertyChanged -= LineItem_PropertyChanged;
                    AccountBalances.Clear();
                    foreach (var acc in result.Data)
                    {
                        var item = new OpeningBalanceLineItem
                        {
                            AccountId = acc.Id,
                            AccountCode = acc.AccountCode,
                            AccountNameAr = acc.AccountNameAr,
                            DebitAmount = 0m,
                            CreditAmount = 0m
                        };
                        item.PropertyChanged += LineItem_PropertyChanged;
                        AccountBalances.Add(item);
                    }
                    StatusMessage = $"تم تحميل {AccountBalances.Count} حساب قابل للترحيل.";
                }
                else
                {
                    ErrorMessage = string.Join("; ", result.Errors);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = FriendlyErrorMessage("تحميل الحسابات", ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void LineItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(OpeningBalanceLineItem.DebitAmount) ||
                e.PropertyName == nameof(OpeningBalanceLineItem.CreditAmount))
            {
                RefreshTotals();
            }
        }

        private void RefreshTotals()
        {
            OnPropertyChanged(nameof(TotalDebit));
            OnPropertyChanged(nameof(TotalCredit));
            OnPropertyChanged(nameof(IsBalanced));
            OnPropertyChanged(nameof(Difference));
        }

        // ── Post Opening Balance ─────────────────────────────────

        private bool CanPost()
        {
            return !IsBusy && IsBalanced && CurrentStep == 3;
        }

        private async Task PostOpeningBalanceAsync()
        {
            IsBusy = true;
            ClearError();
            try
            {
                var nonZeroLines = AccountBalances
                    .Where(x => x.DebitAmount > 0 || x.CreditAmount > 0)
                    .ToList();

                if (nonZeroLines.Count < 2)
                {
                    ErrorMessage = "يجب إدخال رصيد لحسابين على الأقل.";
                    return;
                }

                // Build the DTO
                var dto = new CreateJournalEntryDto
                {
                    JournalDate = SelectedFiscalYear.StartDate,
                    Description = $"أرصدة افتتاحية — السنة المالية {SelectedFiscalYear.Year}",
                    SourceType = SourceType.Opening,
                    ReferenceNumber = $"OB-{SelectedFiscalYear.Year}",
                    Lines = nonZeroLines.Select(line => new CreateJournalEntryLineDto
                    {
                        AccountId = line.AccountId,
                        DebitAmount = line.DebitAmount,
                        CreditAmount = line.CreditAmount,
                        Description = $"رصيد افتتاحي — {line.AccountNameAr}"
                    }).ToList()
                };

                // Step 1: Create draft
                var draftResult = await _journalService.CreateDraftAsync(dto, CancellationToken.None);
                if (!draftResult.IsSuccess)
                {
                    ErrorMessage = string.Join("; ", draftResult.Errors);
                    return;
                }

                // Step 2: Post the draft
                var postResult = await _journalService.PostAsync(draftResult.Data.Id, CancellationToken.None);
                if (!postResult.IsSuccess)
                {
                    ErrorMessage = string.Join("; ", postResult.Errors);
                    return;
                }

                StatusMessage = $"تم ترحيل الأرصدة الافتتاحية بنجاح — رقم القيد: {postResult.Data.JournalNumber}";
                _postedJournalNumber = postResult.Data.JournalNumber;
                OnPropertyChanged(nameof(PostedJournalNumber));
                OnPropertyChanged(nameof(IsPosted));
            }
            catch (Exception ex)
            {
                ErrorMessage = FriendlyErrorMessage("الترحيل", ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private string _postedJournalNumber;
        public string PostedJournalNumber => _postedJournalNumber;
        public bool IsPosted => !string.IsNullOrEmpty(_postedJournalNumber);
    }

    // ── Line Item Model ──────────────────────────────────────

    /// <summary>
    /// Bindable model for a single account row in the opening balance grid.
    /// </summary>
    public sealed class OpeningBalanceLineItem : INotifyPropertyChanged
    {
        public int AccountId { get; set; }
        public string AccountCode { get; set; }
        public string AccountNameAr { get; set; }

        private decimal _debitAmount;
        public decimal DebitAmount
        {
            get => _debitAmount;
            set
            {
                if (_debitAmount != value)
                {
                    _debitAmount = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DebitAmount)));
                }
            }
        }

        private decimal _creditAmount;
        public decimal CreditAmount
        {
            get => _creditAmount;
            set
            {
                if (_creditAmount != value)
                {
                    _creditAmount = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CreditAmount)));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}

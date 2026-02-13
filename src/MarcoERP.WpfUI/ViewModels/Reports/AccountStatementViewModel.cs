using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using MarcoERP.Application.DTOs.Accounting;
using MarcoERP.Application.DTOs.Reports;
using MarcoERP.Application.Interfaces.Accounting;
using MarcoERP.Application.Interfaces.Reports;
using MarcoERP.WpfUI.Common;

namespace MarcoERP.WpfUI.ViewModels.Reports
{
    /// <summary>
    /// ViewModel for the Account Statement report.
    /// </summary>
    public sealed class AccountStatementViewModel : BaseViewModel
    {
        private readonly IReportService _reportService;
        private readonly IAccountService _accountService;
        private readonly IReportExportService _exportService;

        public AccountStatementViewModel(IReportService reportService, IAccountService accountService, IReportExportService exportService)
        {
            _reportService = reportService;
            _accountService = accountService;
            _exportService = exportService;
            FromDate = new DateTime(DateTime.Today.Year, 1, 1);
            ToDate = DateTime.Today;
            Accounts = new ObservableCollection<AccountDto>();
            Rows = new ObservableCollection<AccountStatementRowDto>();
            GenerateCommand = new AsyncRelayCommand(GenerateAsync);
            ExportPdfCommand = new AsyncRelayCommand(ExportPdfAsync);
            ExportExcelCommand = new AsyncRelayCommand(ExportExcelAsync);

            _ = LoadAccountsAsync();
        }

        // ── Filter ──
        private DateTime _fromDate;
        public DateTime FromDate { get => _fromDate; set => SetProperty(ref _fromDate, value); }

        private DateTime _toDate;
        public DateTime ToDate { get => _toDate; set => SetProperty(ref _toDate, value); }

        public ObservableCollection<AccountDto> Accounts { get; }

        private AccountDto _selectedAccount;
        public AccountDto SelectedAccount { get => _selectedAccount; set => SetProperty(ref _selectedAccount, value); }

        // ── Header ──
        private string _accountCode;
        public string AccountCode { get => _accountCode; set => SetProperty(ref _accountCode, value); }

        private string _accountName;
        public string AccountName { get => _accountName; set => SetProperty(ref _accountName, value); }

        private decimal _openingBalance;
        public decimal OpeningBalance { get => _openingBalance; set => SetProperty(ref _openingBalance, value); }

        private decimal _closingBalance;
        public decimal ClosingBalance { get => _closingBalance; set => SetProperty(ref _closingBalance, value); }

        private decimal _totalDebit;
        public decimal TotalDebit { get => _totalDebit; set => SetProperty(ref _totalDebit, value); }

        private decimal _totalCredit;
        public decimal TotalCredit { get => _totalCredit; set => SetProperty(ref _totalCredit, value); }

        // ── Results ──
        public ObservableCollection<AccountStatementRowDto> Rows { get; }

        public ICommand GenerateCommand { get; }
        public ICommand ExportPdfCommand { get; }
        public ICommand ExportExcelCommand { get; }

        private ReportExportRequest BuildExportRequest()
        {
            var req = new ReportExportRequest
            {
                Title = $"كشف حساب: {AccountName}",
                Subtitle = $"من {FromDate:yyyy/MM/dd} إلى {ToDate:yyyy/MM/dd}  |  الرصيد الافتتاحي: {OpeningBalance:N2}",
                Columns = new List<ReportColumn>
                {
                    new("التاريخ", 1f),
                    new("رقم القيد", 1f),
                    new("البيان", 2f),
                    new("النوع", 1f),
                    new("مدين", 1.2f, true),
                    new("دائن", 1.2f, true),
                    new("الرصيد", 1.2f, true),
                },
                FooterSummary = $"إجمالي المدين: {TotalDebit:N2}  |  إجمالي الدائن: {TotalCredit:N2}  |  الرصيد الختامي: {ClosingBalance:N2}"
            };
            foreach (var r in Rows)
                req.Rows.Add(new List<string> { r.Date.ToString("yyyy/MM/dd"), r.JournalNumber, r.Description, r.SourceTypeName, r.DebitAmount.ToString("N2"), r.CreditAmount.ToString("N2"), r.RunningBalance.ToString("N2") });
            return req;
        }

        private async Task ExportPdfAsync()
        {
            if (Rows.Count == 0) { ErrorMessage = "لا توجد بيانات للتصدير."; return; }
            var result = await ReportExportHelper.ExportPdfAsync(_exportService, BuildExportRequest());
            if (result != null) StatusMessage = result.EndsWith(".pdf") ? "تم التصدير بنجاح" : result;
        }

        private async Task ExportExcelAsync()
        {
            if (Rows.Count == 0) { ErrorMessage = "لا توجد بيانات للتصدير."; return; }
            var result = await ReportExportHelper.ExportExcelAsync(_exportService, BuildExportRequest());
            if (result != null) StatusMessage = result.EndsWith(".xlsx") ? "تم التصدير بنجاح" : result;
        }

        private async Task LoadAccountsAsync()
        {
            var result = await _accountService.GetPostableAccountsAsync();
            if (result.IsSuccess && result.Data != null)
            {
                Accounts.Clear();
                foreach (var acc in result.Data)
                    Accounts.Add(acc);
            }
        }

        private async Task GenerateAsync()
        {
            if (SelectedAccount == null)
            {
                ErrorMessage = "الرجاء اختيار حساب.";
                return;
            }
            if (IsBusy) return;
            IsBusy = true;
            ClearError();

            try
            {
                var result = await _reportService.GetAccountStatementAsync(
                    SelectedAccount.Id, FromDate, ToDate);

                Rows.Clear();

                if (result.IsSuccess && result.Data != null)
                {
                    var d = result.Data;
                    AccountCode = d.AccountCode;
                    AccountName = d.AccountNameAr;
                    OpeningBalance = d.OpeningBalance;
                    ClosingBalance = d.ClosingBalance;
                    TotalDebit = d.TotalDebit;
                    TotalCredit = d.TotalCredit;

                    foreach (var row in d.Rows)
                        Rows.Add(row);

                    StatusMessage = $"تم عرض {Rows.Count} حركة";
                }
                else
                {
                    ErrorMessage = result.ErrorMessage ?? "فشل إنشاء التقرير.";
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
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using MarcoERP.Application.DTOs.Reports;
using MarcoERP.Application.Interfaces.Reports;
using MarcoERP.WpfUI.Common;

namespace MarcoERP.WpfUI.ViewModels.Reports
{
    /// <summary>
    /// ViewModel for the Trial Balance report.
    /// </summary>
    public sealed class TrialBalanceViewModel : BaseViewModel
    {
        private readonly IReportService _reportService;
        private readonly IReportExportService _exportService;

        public TrialBalanceViewModel(IReportService reportService, IReportExportService exportService)
        {
            _reportService = reportService;
            _exportService = exportService;
            FromDate = new DateTime(DateTime.Today.Year, 1, 1);
            ToDate = DateTime.Today;
            Rows = new ObservableCollection<TrialBalanceRowDto>();
            GenerateCommand = new AsyncRelayCommand(GenerateAsync);
            ExportPdfCommand = new AsyncRelayCommand(ExportPdfAsync);
            ExportExcelCommand = new AsyncRelayCommand(ExportExcelAsync);
        }

        // ── Filter Properties ──
        private DateTime _fromDate;
        public DateTime FromDate { get => _fromDate; set => SetProperty(ref _fromDate, value); }

        private DateTime _toDate;
        public DateTime ToDate { get => _toDate; set => SetProperty(ref _toDate, value); }

        // ── Results ──
        public ObservableCollection<TrialBalanceRowDto> Rows { get; }

        private decimal _totalDebit;
        public decimal TotalDebit { get => _totalDebit; set => SetProperty(ref _totalDebit, value); }

        private decimal _totalCredit;
        public decimal TotalCredit { get => _totalCredit; set => SetProperty(ref _totalCredit, value); }

        // ── Commands ──
        public ICommand GenerateCommand { get; }
        public ICommand ExportPdfCommand { get; }
        public ICommand ExportExcelCommand { get; }

        private ReportExportRequest BuildExportRequest()
        {
            var req = new ReportExportRequest
            {
                Title = "ميزان المراجعة",
                Subtitle = $"من {FromDate:yyyy/MM/dd} إلى {ToDate:yyyy/MM/dd}",
                Columns = new List<ReportColumn>
                {
                    new("كود الحساب", 1f),
                    new("اسم الحساب", 2f),
                    new("نوع الحساب", 1f),
                    new("إجمالي المدين", 1.2f, true),
                    new("إجمالي الدائن", 1.2f, true),
                    new("الرصيد", 1.2f, true),
                    new("الاتجاه", 0.8f),
                },
                FooterSummary = $"إجمالي المدين: {TotalDebit:N2}  |  إجمالي الدائن: {TotalCredit:N2}"
            };
            foreach (var r in Rows)
                req.Rows.Add(new List<string> { r.AccountCode, r.AccountNameAr, r.AccountTypeName, r.TotalDebit.ToString("N2"), r.TotalCredit.ToString("N2"), r.Balance.ToString("N2"), r.BalanceSide });
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

        private async Task GenerateAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            ClearError();

            try
            {
                var result = await _reportService.GetTrialBalanceAsync(FromDate, ToDate);
                Rows.Clear();

                if (result.IsSuccess && result.Data != null)
                {
                    foreach (var row in result.Data)
                        Rows.Add(row);

                    TotalDebit = result.Data.Sum(r => r.TotalDebit);
                    TotalCredit = result.Data.Sum(r => r.TotalCredit);
                    StatusMessage = $"تم عرض {Rows.Count} حساب";
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

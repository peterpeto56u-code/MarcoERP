using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using MarcoERP.Application.DTOs.Reports;
using MarcoERP.Application.DTOs.Treasury;
using MarcoERP.Application.Interfaces.Reports;
using MarcoERP.Application.Interfaces.Treasury;
using MarcoERP.WpfUI.Common;

namespace MarcoERP.WpfUI.ViewModels.Reports
{
    public sealed class CashboxMovementViewModel : BaseViewModel
    {
        private readonly IReportService _reportService;
        private readonly ICashboxService _cashboxService;
        private readonly IReportExportService _exportService;

        public CashboxMovementViewModel(IReportService reportService, ICashboxService cashboxService, IReportExportService exportService)
        {
            _reportService = reportService;
            _cashboxService = cashboxService;
            _exportService = exportService;
            FromDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            ToDate = DateTime.Today;
            Cashboxes = new ObservableCollection<CashboxDto>();
            Rows = new ObservableCollection<CashboxMovementRowDto>();
            GenerateCommand = new AsyncRelayCommand(GenerateAsync);
            ExportPdfCommand = new AsyncRelayCommand(ExportPdfAsync);
            ExportExcelCommand = new AsyncRelayCommand(ExportExcelAsync);
            _ = LoadCashboxesAsync();
        }

        private DateTime _fromDate;
        public DateTime FromDate { get => _fromDate; set => SetProperty(ref _fromDate, value); }
        private DateTime _toDate;
        public DateTime ToDate { get => _toDate; set => SetProperty(ref _toDate, value); }
        public ObservableCollection<CashboxDto> Cashboxes { get; }
        private CashboxDto _selectedCashbox;
        public CashboxDto SelectedCashbox { get => _selectedCashbox; set => SetProperty(ref _selectedCashbox, value); }
        public ObservableCollection<CashboxMovementRowDto> Rows { get; }
        private string _cashboxName;
        public string CashboxName { get => _cashboxName; set => SetProperty(ref _cashboxName, value); }
        private decimal _openingBalance;
        public decimal OpeningBalance { get => _openingBalance; set => SetProperty(ref _openingBalance, value); }
        private decimal _totalIn;
        public decimal TotalIn { get => _totalIn; set => SetProperty(ref _totalIn, value); }
        private decimal _totalOut;
        public decimal TotalOut { get => _totalOut; set => SetProperty(ref _totalOut, value); }
        private decimal _closingBalance;
        public decimal ClosingBalance { get => _closingBalance; set => SetProperty(ref _closingBalance, value); }
        public ICommand GenerateCommand { get; }
        public ICommand ExportPdfCommand { get; }
        public ICommand ExportExcelCommand { get; }

        private ReportExportRequest BuildExportRequest()
        {
            var req = new ReportExportRequest
            {
                Title = $"حركة الصندوق: {CashboxName}",
                Subtitle = $"من {FromDate:yyyy/MM/dd} إلى {ToDate:yyyy/MM/dd}  |  الرصيد الافتتاحي: {OpeningBalance:N2}",
                Columns = new List<ReportColumn>
                {
                    new("التاريخ", 1f),
                    new("نوع المستند", 1f),
                    new("رقم المستند", 1f),
                    new("البيان", 1.5f),
                    new("الطرف", 1.2f),
                    new("وارد", 1.2f, true),
                    new("صادر", 1.2f, true),
                    new("الرصيد", 1.2f, true),
                },
                FooterSummary = $"إجمالي الوارد: {TotalIn:N2}  |  إجمالي الصادر: {TotalOut:N2}  |  الرصيد الختامي: {ClosingBalance:N2}"
            };
            foreach (var r in Rows)
                req.Rows.Add(new List<string> { r.Date.ToString("yyyy/MM/dd"), r.DocumentType, r.DocumentNumber, r.Description, r.CounterpartyName, r.AmountIn.ToString("N2"), r.AmountOut.ToString("N2"), r.RunningBalance.ToString("N2") });
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

        private async Task LoadCashboxesAsync()
        {
            var result = await _cashboxService.GetAllAsync();
            if (result.IsSuccess && result.Data != null)
                foreach (var c in result.Data) Cashboxes.Add(c);
        }

        private async Task GenerateAsync()
        {
            if (IsBusy) return;
            IsBusy = true; ClearError();
            try
            {
                var result = await _reportService.GetCashboxMovementAsync(SelectedCashbox?.Id, FromDate, ToDate);
                Rows.Clear();
                if (result.IsSuccess && result.Data != null)
                {
                    var d = result.Data;
                    CashboxName = d.CashboxName;
                    OpeningBalance = d.OpeningBalance;
                    TotalIn = d.TotalIn;
                    TotalOut = d.TotalOut;
                    ClosingBalance = d.ClosingBalance;
                    foreach (var r in d.Rows) Rows.Add(r);
                    StatusMessage = $"تم عرض {d.Rows.Count} حركة";
                }
                else ErrorMessage = result.ErrorMessage ?? "فشل إنشاء التقرير.";
            }
            catch (Exception ex) { ErrorMessage = FriendlyErrorMessage("العملية", ex); }
            finally { IsBusy = false; }
        }
    }
}

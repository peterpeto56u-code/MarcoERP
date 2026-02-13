using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using MarcoERP.Application.DTOs.Reports;
using MarcoERP.Application.Interfaces.Reports;
using MarcoERP.WpfUI.Common;

namespace MarcoERP.WpfUI.ViewModels.Reports
{
    public sealed class ProfitReportViewModel : BaseViewModel
    {
        private readonly IReportService _reportService;
        private readonly IReportExportService _exportService;

        public ProfitReportViewModel(IReportService reportService, IReportExportService exportService)
        {
            _reportService = reportService;
            _exportService = exportService;
            FromDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            ToDate = DateTime.Today;
            Rows = new ObservableCollection<ProfitReportRowDto>();
            GenerateCommand = new AsyncRelayCommand(GenerateAsync);
            ExportPdfCommand = new AsyncRelayCommand(ExportPdfAsync);
            ExportExcelCommand = new AsyncRelayCommand(ExportExcelAsync);
        }

        private DateTime _fromDate;
        public DateTime FromDate { get => _fromDate; set => SetProperty(ref _fromDate, value); }
        private DateTime _toDate;
        public DateTime ToDate { get => _toDate; set => SetProperty(ref _toDate, value); }
        public ObservableCollection<ProfitReportRowDto> Rows { get; }
        private decimal _totalSales;
        public decimal TotalSales { get => _totalSales; set => SetProperty(ref _totalSales, value); }
        private decimal _totalCost;
        public decimal TotalCost { get => _totalCost; set => SetProperty(ref _totalCost, value); }
        private decimal _totalProfit;
        public decimal TotalProfit { get => _totalProfit; set => SetProperty(ref _totalProfit, value); }
        private decimal _overallMargin;
        public decimal OverallMargin { get => _overallMargin; set => SetProperty(ref _overallMargin, value); }
        public ICommand GenerateCommand { get; }
        public ICommand ExportPdfCommand { get; }
        public ICommand ExportExcelCommand { get; }

        private ReportExportRequest BuildExportRequest()
        {
            var req = new ReportExportRequest
            {
                Title = "تقرير الأرباح",
                Subtitle = $"من {FromDate:yyyy/MM/dd} إلى {ToDate:yyyy/MM/dd}",
                Columns = new List<ReportColumn>
                {
                    new("كود الصنف", 1f),
                    new("اسم الصنف", 1.5f),
                    new("الكمية المباعة", 1f, true),
                    new("إجمالي المبيعات", 1.2f, true),
                    new("إجمالي التكلفة", 1.2f, true),
                    new("الربح", 1.2f, true),
                    new("هامش الربح %", 1f, true),
                },
                FooterSummary = $"إجمالي المبيعات: {TotalSales:N2}  |  إجمالي التكلفة: {TotalCost:N2}  |  صافي الربح: {TotalProfit:N2}  |  هامش الربح: {OverallMargin:N2}%"
            };
            foreach (var r in Rows)
                req.Rows.Add(new List<string> { r.ProductCode, r.ProductName, r.TotalSalesQuantity.ToString("N2"), r.TotalSalesAmount.ToString("N2"), r.TotalCostAmount.ToString("N2"), r.GrossProfit.ToString("N2"), r.ProfitMarginPercent.ToString("N2") });
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
            IsBusy = true; ClearError();
            try
            {
                var result = await _reportService.GetProfitReportAsync(FromDate, ToDate);
                Rows.Clear();
                if (result.IsSuccess && result.Data != null)
                {
                    var d = result.Data;
                    foreach (var r in d.Rows) Rows.Add(r);
                    TotalSales = d.TotalSales;
                    TotalCost = d.TotalCost;
                    TotalProfit = d.TotalProfit;
                    OverallMargin = d.OverallMarginPercent;
                    StatusMessage = $"تم عرض {d.Rows.Count} صنف";
                }
                else ErrorMessage = result.ErrorMessage ?? "فشل إنشاء التقرير.";
            }
            catch (Exception ex) { ErrorMessage = FriendlyErrorMessage("العملية", ex); }
            finally { IsBusy = false; }
        }
    }
}

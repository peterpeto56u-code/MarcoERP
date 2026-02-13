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
    public sealed class VatReportViewModel : BaseViewModel
    {
        private readonly IReportService _reportService;
        private readonly IReportExportService _exportService;

        public VatReportViewModel(IReportService reportService, IReportExportService exportService)
        {
            _reportService = reportService;
            _exportService = exportService;
            FromDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            ToDate = DateTime.Today;
            Rows = new ObservableCollection<VatReportRowDto>();
            GenerateCommand = new AsyncRelayCommand(GenerateAsync);
            ExportPdfCommand = new AsyncRelayCommand(ExportPdfAsync);
            ExportExcelCommand = new AsyncRelayCommand(ExportExcelAsync);
        }

        private DateTime _fromDate;
        public DateTime FromDate { get => _fromDate; set => SetProperty(ref _fromDate, value); }
        private DateTime _toDate;
        public DateTime ToDate { get => _toDate; set => SetProperty(ref _toDate, value); }
        public ObservableCollection<VatReportRowDto> Rows { get; }

        private decimal _totalSalesVat;
        public decimal TotalSalesVat { get => _totalSalesVat; set => SetProperty(ref _totalSalesVat, value); }
        private decimal _totalPurchaseVat;
        public decimal TotalPurchaseVat { get => _totalPurchaseVat; set => SetProperty(ref _totalPurchaseVat, value); }
        private decimal _netVatPayable;
        public decimal NetVatPayable { get => _netVatPayable; set => SetProperty(ref _netVatPayable, value); }

        public ICommand GenerateCommand { get; }
        public ICommand ExportPdfCommand { get; }
        public ICommand ExportExcelCommand { get; }

        private ReportExportRequest BuildExportRequest()
        {
            var req = new ReportExportRequest
            {
                Title = "تقرير ضريبة القيمة المضافة",
                Subtitle = $"من {FromDate:yyyy/MM/dd} إلى {ToDate:yyyy/MM/dd}",
                Columns = new List<ReportColumn>
                {
                    new("نسبة الضريبة %", 1f, true),
                    new("وعاء المبيعات", 1.2f, true),
                    new("ضريبة المبيعات", 1.2f, true),
                    new("وعاء المشتريات", 1.2f, true),
                    new("ضريبة المشتريات", 1.2f, true),
                    new("صافي الضريبة", 1.2f, true),
                },
                FooterSummary = $"ضريبة المبيعات: {TotalSalesVat:N2}  |  ضريبة المشتريات: {TotalPurchaseVat:N2}  |  صافي المستحق: {NetVatPayable:N2}"
            };
            foreach (var r in Rows)
                req.Rows.Add(new List<string> { r.VatRate.ToString("N2"), r.SalesBase.ToString("N2"), r.SalesVat.ToString("N2"), r.PurchaseBase.ToString("N2"), r.PurchaseVat.ToString("N2"), r.NetVat.ToString("N2") });
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
                var result = await _reportService.GetVatReportAsync(FromDate, ToDate);
                Rows.Clear();
                if (result.IsSuccess && result.Data != null)
                {
                    var d = result.Data;
                    foreach (var r in d.Rows) Rows.Add(r);
                    TotalSalesVat = d.TotalSalesVat;
                    TotalPurchaseVat = d.TotalPurchaseVat;
                    NetVatPayable = d.NetVatPayable;
                    StatusMessage = "تم إنشاء تقرير الضريبة";
                }
                else ErrorMessage = result.ErrorMessage ?? "فشل إنشاء التقرير.";
            }
            catch (Exception ex) { ErrorMessage = FriendlyErrorMessage("العملية", ex); }
            finally { IsBusy = false; }
        }
    }
}

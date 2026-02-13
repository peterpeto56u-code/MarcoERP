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
    public sealed class AgingReportViewModel : BaseViewModel
    {
        private readonly IReportService _reportService;
        private readonly IReportExportService _exportService;

        public AgingReportViewModel(IReportService reportService, IReportExportService exportService)
        {
            _reportService = reportService;
            _exportService = exportService;
            CustomerRows = new ObservableCollection<AgingRowDto>();
            SupplierRows = new ObservableCollection<AgingRowDto>();
            GenerateCommand = new AsyncRelayCommand(GenerateAsync);
            ExportPdfCommand = new AsyncRelayCommand(ExportPdfAsync);
            ExportExcelCommand = new AsyncRelayCommand(ExportExcelAsync);
        }

        public ObservableCollection<AgingRowDto> CustomerRows { get; }
        public ObservableCollection<AgingRowDto> SupplierRows { get; }

        private decimal _totalCustomerBalance;
        public decimal TotalCustomerBalance { get => _totalCustomerBalance; set => SetProperty(ref _totalCustomerBalance, value); }
        private decimal _totalSupplierBalance;
        public decimal TotalSupplierBalance { get => _totalSupplierBalance; set => SetProperty(ref _totalSupplierBalance, value); }

        public ICommand GenerateCommand { get; }
        public ICommand ExportPdfCommand { get; }
        public ICommand ExportExcelCommand { get; }

        private ReportExportRequest BuildExportRequest()
        {
            var columns = new List<ReportColumn>
            {
                new("الكود", 0.8f),
                new("الاسم", 1.5f),
                new("جاري (0-30)", 1f, true),
                new("31-60 يوم", 1f, true),
                new("61-90 يوم", 1f, true),
                new("91-120 يوم", 1f, true),
                new("+120 يوم", 1f, true),
                new("الإجمالي", 1.2f, true),
            };

            var req = new ReportExportRequest
            {
                Title = "تقرير أعمار الديون",
                Subtitle = $"بتاريخ {DateTime.Today:yyyy/MM/dd}",
                Columns = columns,
                FooterSummary = $"إجمالي رصيد العملاء: {TotalCustomerBalance:N2}  |  إجمالي رصيد الموردين: {TotalSupplierBalance:N2}"
            };

            // Section header for customers
            req.Rows.Add(new List<string> { "──", "── العملاء ──", "", "", "", "", "", "" });
            foreach (var r in CustomerRows)
                req.Rows.Add(new List<string> { r.Code, r.Name, r.Current.ToString("N2"), r.Days30.ToString("N2"), r.Days60.ToString("N2"), r.Days90.ToString("N2"), r.Days120Plus.ToString("N2"), r.Total.ToString("N2") });

            // Section header for suppliers
            req.Rows.Add(new List<string> { "──", "── الموردين ──", "", "", "", "", "", "" });
            foreach (var r in SupplierRows)
                req.Rows.Add(new List<string> { r.Code, r.Name, r.Current.ToString("N2"), r.Days30.ToString("N2"), r.Days60.ToString("N2"), r.Days90.ToString("N2"), r.Days120Plus.ToString("N2"), r.Total.ToString("N2") });

            return req;
        }

        private async Task ExportPdfAsync()
        {
            if (CustomerRows.Count == 0 && SupplierRows.Count == 0) { ErrorMessage = "لا توجد بيانات للتصدير."; return; }
            var result = await ReportExportHelper.ExportPdfAsync(_exportService, BuildExportRequest());
            if (result != null) StatusMessage = result.EndsWith(".pdf") ? "تم التصدير بنجاح" : result;
        }

        private async Task ExportExcelAsync()
        {
            if (CustomerRows.Count == 0 && SupplierRows.Count == 0) { ErrorMessage = "لا توجد بيانات للتصدير."; return; }
            var result = await ReportExportHelper.ExportExcelAsync(_exportService, BuildExportRequest());
            if (result != null) StatusMessage = result.EndsWith(".xlsx") ? "تم التصدير بنجاح" : result;
        }

        private async Task GenerateAsync()
        {
            if (IsBusy) return;
            IsBusy = true; ClearError();
            try
            {
                var result = await _reportService.GetAgingReportAsync();
                CustomerRows.Clear(); SupplierRows.Clear();
                if (result.IsSuccess && result.Data != null)
                {
                    var d = result.Data;
                    foreach (var r in d.CustomerAging) CustomerRows.Add(r);
                    foreach (var r in d.SupplierAging) SupplierRows.Add(r);
                    TotalCustomerBalance = d.TotalCustomerBalance;
                    TotalSupplierBalance = d.TotalSupplierBalance;
                    StatusMessage = $"عملاء: {d.CustomerAging.Count}، موردين: {d.SupplierAging.Count}";
                }
                else ErrorMessage = result.ErrorMessage ?? "فشل إنشاء التقرير.";
            }
            catch (Exception ex) { ErrorMessage = FriendlyErrorMessage("العملية", ex); }
            finally { IsBusy = false; }
        }
    }
}

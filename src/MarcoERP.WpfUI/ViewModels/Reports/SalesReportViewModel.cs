using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using MarcoERP.Application.DTOs.Reports;
using MarcoERP.Application.DTOs.Sales;
using MarcoERP.Application.Interfaces.Reports;
using MarcoERP.Application.Interfaces.Sales;
using MarcoERP.WpfUI.Common;

namespace MarcoERP.WpfUI.ViewModels.Reports
{
    public sealed class SalesReportViewModel : BaseViewModel
    {
        private readonly IReportService _reportService;
        private readonly ICustomerService _customerService;
        private readonly IReportExportService _exportService;

        public SalesReportViewModel(IReportService reportService, ICustomerService customerService, IReportExportService exportService)
        {
            _reportService = reportService;
            _customerService = customerService;
            _exportService = exportService;
            FromDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            ToDate = DateTime.Today;
            Customers = new ObservableCollection<CustomerDto>();
            Rows = new ObservableCollection<SalesReportRowDto>();
            GenerateCommand = new AsyncRelayCommand(GenerateAsync);
            ExportPdfCommand = new AsyncRelayCommand(ExportPdfAsync);
            ExportExcelCommand = new AsyncRelayCommand(ExportExcelAsync);
            _ = LoadCustomersAsync();
        }

        private DateTime _fromDate;
        public DateTime FromDate { get => _fromDate; set => SetProperty(ref _fromDate, value); }

        private DateTime _toDate;
        public DateTime ToDate { get => _toDate; set => SetProperty(ref _toDate, value); }

        public ObservableCollection<CustomerDto> Customers { get; }

        private CustomerDto _selectedCustomer;
        public CustomerDto SelectedCustomer { get => _selectedCustomer; set => SetProperty(ref _selectedCustomer, value); }

        public ObservableCollection<SalesReportRowDto> Rows { get; }

        private int _invoiceCount;
        public int InvoiceCount { get => _invoiceCount; set => SetProperty(ref _invoiceCount, value); }
        private decimal _totalNet;
        public decimal TotalNet { get => _totalNet; set => SetProperty(ref _totalNet, value); }
        private decimal _totalVat;
        public decimal TotalVat { get => _totalVat; set => SetProperty(ref _totalVat, value); }
        private decimal _totalDiscount;
        public decimal TotalDiscount { get => _totalDiscount; set => SetProperty(ref _totalDiscount, value); }

        public ICommand GenerateCommand { get; }
        public ICommand ExportPdfCommand { get; }
        public ICommand ExportExcelCommand { get; }

        private ReportExportRequest BuildExportRequest()
        {
            var req = new ReportExportRequest
            {
                Title = "تقرير المبيعات",
                Subtitle = $"من {FromDate:yyyy/MM/dd} إلى {ToDate:yyyy/MM/dd}",
                Columns = new List<ReportColumn>
                {
                    new("رقم الفاتورة", 1f),
                    new("التاريخ", 1f),
                    new("العميل", 1.5f),
                    new("الحالة", 0.8f),
                    new("المبلغ", 1.2f, true),
                    new("الخصم", 1f, true),
                    new("الضريبة", 1f, true),
                    new("الصافي", 1.2f, true),
                },
                FooterSummary = $"عدد الفواتير: {InvoiceCount}  |  الصافي: {TotalNet:N2}  |  الضريبة: {TotalVat:N2}  |  الخصم: {TotalDiscount:N2}"
            };
            foreach (var r in Rows)
                req.Rows.Add(new List<string> { r.InvoiceNumber, r.InvoiceDate.ToString("yyyy/MM/dd"), r.CustomerName, r.Status, r.Subtotal.ToString("N2"), r.DiscountTotal.ToString("N2"), r.VatTotal.ToString("N2"), r.NetTotal.ToString("N2") });
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

        private async Task LoadCustomersAsync()
        {
            var result = await _customerService.GetAllAsync();
            if (result.IsSuccess && result.Data != null)
                foreach (var c in result.Data) Customers.Add(c);
        }

        private async Task GenerateAsync()
        {
            if (IsBusy) return;
            IsBusy = true; ClearError();
            try
            {
                var result = await _reportService.GetSalesReportAsync(FromDate, ToDate, SelectedCustomer?.Id);
                Rows.Clear();
                if (result.IsSuccess && result.Data != null)
                {
                    foreach (var r in result.Data.Rows) Rows.Add(r);
                    InvoiceCount = result.Data.InvoiceCount;
                    TotalNet = result.Data.TotalNet;
                    TotalVat = result.Data.TotalVat;
                    TotalDiscount = result.Data.TotalDiscount;
                    StatusMessage = $"تم عرض {InvoiceCount} فاتورة";
                }
                else ErrorMessage = result.ErrorMessage ?? "فشل إنشاء التقرير.";
            }
            catch (Exception ex) { ErrorMessage = FriendlyErrorMessage("العملية", ex); }
            finally { IsBusy = false; }
        }
    }
}

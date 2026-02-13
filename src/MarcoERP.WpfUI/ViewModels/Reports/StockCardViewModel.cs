using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using MarcoERP.Application.DTOs.Inventory;
using MarcoERP.Application.DTOs.Reports;
using MarcoERP.Application.Interfaces.Inventory;
using MarcoERP.Application.Interfaces.Reports;
using MarcoERP.WpfUI.Common;

namespace MarcoERP.WpfUI.ViewModels.Reports
{
    public sealed class StockCardViewModel : BaseViewModel
    {
        private readonly IReportService _reportService;
        private readonly IProductService _productService;
        private readonly IWarehouseService _warehouseService;
        private readonly IReportExportService _exportService;

        public StockCardViewModel(IReportService reportService, IProductService productService, IWarehouseService warehouseService, IReportExportService exportService)
        {
            _reportService = reportService;
            _productService = productService;
            _warehouseService = warehouseService;
            _exportService = exportService;
            FromDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            ToDate = DateTime.Today;
            Products = new ObservableCollection<ProductDto>();
            Warehouses = new ObservableCollection<WarehouseDto>();
            Rows = new ObservableCollection<StockCardRowDto>();
            GenerateCommand = new AsyncRelayCommand(GenerateAsync);
            ExportPdfCommand = new AsyncRelayCommand(ExportPdfAsync);
            ExportExcelCommand = new AsyncRelayCommand(ExportExcelAsync);
            _ = LoadLookupsAsync();
        }

        private DateTime _fromDate;
        public DateTime FromDate { get => _fromDate; set => SetProperty(ref _fromDate, value); }
        private DateTime _toDate;
        public DateTime ToDate { get => _toDate; set => SetProperty(ref _toDate, value); }
        public ObservableCollection<ProductDto> Products { get; }
        private ProductDto _selectedProduct;
        public ProductDto SelectedProduct { get => _selectedProduct; set => SetProperty(ref _selectedProduct, value); }
        public ObservableCollection<WarehouseDto> Warehouses { get; }
        private WarehouseDto _selectedWarehouse;
        public WarehouseDto SelectedWarehouse { get => _selectedWarehouse; set => SetProperty(ref _selectedWarehouse, value); }
        public ObservableCollection<StockCardRowDto> Rows { get; }

        private string _productName;
        public string ProductName { get => _productName; set => SetProperty(ref _productName, value); }
        private string _unitName;
        public string UnitName { get => _unitName; set => SetProperty(ref _unitName, value); }
        private decimal _openingBalance;
        public decimal OpeningBalance { get => _openingBalance; set => SetProperty(ref _openingBalance, value); }
        private decimal _closingBalance;
        public decimal ClosingBalance { get => _closingBalance; set => SetProperty(ref _closingBalance, value); }
        private decimal _totalIn;
        public decimal TotalIn { get => _totalIn; set => SetProperty(ref _totalIn, value); }
        private decimal _totalOut;
        public decimal TotalOut { get => _totalOut; set => SetProperty(ref _totalOut, value); }

        public ICommand GenerateCommand { get; }
        public ICommand ExportPdfCommand { get; }
        public ICommand ExportExcelCommand { get; }

        private ReportExportRequest BuildExportRequest()
        {
            var req = new ReportExportRequest
            {
                Title = $"بطاقة صنف: {ProductName}",
                Subtitle = $"من {FromDate:yyyy/MM/dd} إلى {ToDate:yyyy/MM/dd}  |  الوحدة: {UnitName}  |  الرصيد الافتتاحي: {OpeningBalance:N2}",
                Columns = new List<ReportColumn>
                {
                    new("التاريخ", 1f),
                    new("نوع الحركة", 1f),
                    new("المرجع", 1f),
                    new("المصدر", 1f),
                    new("المستودع", 1f),
                    new("وارد", 1f, true),
                    new("صادر", 1f, true),
                    new("سعر الوحدة", 1f, true),
                    new("الرصيد", 1f, true),
                },
                FooterSummary = $"إجمالي الوارد: {TotalIn:N2}  |  إجمالي الصادر: {TotalOut:N2}  |  الرصيد الختامي: {ClosingBalance:N2}"
            };
            foreach (var r in Rows)
                req.Rows.Add(new List<string> { r.MovementDate.ToString("yyyy/MM/dd"), r.MovementTypeName, r.ReferenceNumber, r.SourceTypeName, r.WarehouseName, r.QuantityIn.ToString("N2"), r.QuantityOut.ToString("N2"), r.UnitCost.ToString("N2"), r.BalanceAfter.ToString("N2") });
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

        private async Task LoadLookupsAsync()
        {
            var pResult = await _productService.GetAllAsync();
            if (pResult.IsSuccess && pResult.Data != null)
                foreach (var p in pResult.Data) Products.Add(p);
            var wResult = await _warehouseService.GetAllAsync();
            if (wResult.IsSuccess && wResult.Data != null)
                foreach (var w in wResult.Data) Warehouses.Add(w);
        }

        private async Task GenerateAsync()
        {
            if (IsBusy) return;
            if (SelectedProduct == null) { ErrorMessage = "يرجى اختيار صنف."; return; }
            IsBusy = true; ClearError();
            try
            {
                var result = await _reportService.GetStockCardAsync(SelectedProduct.Id, SelectedWarehouse?.Id, FromDate, ToDate);
                Rows.Clear();
                if (result.IsSuccess && result.Data != null)
                {
                    var d = result.Data;
                    ProductName = d.ProductName;
                    UnitName = d.UnitName;
                    OpeningBalance = d.OpeningBalance;
                    ClosingBalance = d.ClosingBalance;
                    TotalIn = d.TotalIn;
                    TotalOut = d.TotalOut;
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

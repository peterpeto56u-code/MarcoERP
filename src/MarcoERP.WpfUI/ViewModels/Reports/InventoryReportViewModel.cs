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
    public sealed class InventoryReportViewModel : BaseViewModel
    {
        private readonly IReportService _reportService;
        private readonly IWarehouseService _warehouseService;
        private readonly IReportExportService _exportService;

        public InventoryReportViewModel(IReportService reportService, IWarehouseService warehouseService, IReportExportService exportService)
        {
            _reportService = reportService;
            _warehouseService = warehouseService;
            _exportService = exportService;
            Warehouses = new ObservableCollection<WarehouseDto>();
            Rows = new ObservableCollection<InventoryReportRowDto>();
            GenerateCommand = new AsyncRelayCommand(GenerateAsync);
            ExportPdfCommand = new AsyncRelayCommand(ExportPdfAsync);
            ExportExcelCommand = new AsyncRelayCommand(ExportExcelAsync);
            _ = LoadWarehousesAsync();
        }

        public ObservableCollection<WarehouseDto> Warehouses { get; }

        private WarehouseDto _selectedWarehouse;
        public WarehouseDto SelectedWarehouse { get => _selectedWarehouse; set => SetProperty(ref _selectedWarehouse, value); }

        public ObservableCollection<InventoryReportRowDto> Rows { get; }

        private int _totalItems;
        public int TotalItems { get => _totalItems; set => SetProperty(ref _totalItems, value); }

        private decimal _totalValue;
        public decimal TotalValue { get => _totalValue; set => SetProperty(ref _totalValue, value); }

        private int _belowMinCount;
        public int BelowMinCount { get => _belowMinCount; set => SetProperty(ref _belowMinCount, value); }

        public ICommand GenerateCommand { get; }
        public ICommand ExportPdfCommand { get; }
        public ICommand ExportExcelCommand { get; }

        private ReportExportRequest BuildExportRequest()
        {
            var req = new ReportExportRequest
            {
                Title = "تقرير المخزون",
                Subtitle = SelectedWarehouse != null ? $"المستودع: {SelectedWarehouse.NameAr}" : "جميع المستودعات",
                Columns = new List<ReportColumn>
                {
                    new("كود الصنف", 1f),
                    new("اسم الصنف", 1.5f),
                    new("التصنيف", 1f),
                    new("المستودع", 1f),
                    new("الوحدة", 0.8f),
                    new("الكمية", 1f, true),
                    new("سعر التكلفة", 1f, true),
                    new("إجمالي القيمة", 1.2f, true),
                    new("الحد الأدنى", 1f, true),
                },
                FooterSummary = $"عدد الأصناف: {TotalItems}  |  إجمالي القيمة: {TotalValue:N2}  |  أقل من الحد الأدنى: {BelowMinCount}"
            };
            foreach (var r in Rows)
                req.Rows.Add(new List<string> { r.ProductCode, r.ProductName, r.CategoryName, r.WarehouseName, r.UnitName, r.Quantity.ToString("N2"), r.CostPrice.ToString("N2"), r.TotalValue.ToString("N2"), r.MinimumStock.ToString("N2") });
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

        private async Task LoadWarehousesAsync()
        {
            var result = await _warehouseService.GetAllAsync();
            if (result.IsSuccess && result.Data != null)
                foreach (var w in result.Data) Warehouses.Add(w);
        }

        private async Task GenerateAsync()
        {
            if (IsBusy) return;
            IsBusy = true; ClearError();
            try
            {
                var result = await _reportService.GetInventoryReportAsync(SelectedWarehouse?.Id);
                Rows.Clear();
                if (result.IsSuccess && result.Data != null)
                {
                    decimal total = 0; int below = 0;
                    foreach (var r in result.Data)
                    {
                        Rows.Add(r);
                        total += r.TotalValue;
                        if (r.IsBelowMinimum) below++;
                    }
                    TotalItems = result.Data.Count;
                    TotalValue = total;
                    BelowMinCount = below;
                    StatusMessage = $"تم عرض {TotalItems} صنف";
                }
                else ErrorMessage = result.ErrorMessage ?? "فشل إنشاء التقرير.";
            }
            catch (Exception ex) { ErrorMessage = FriendlyErrorMessage("العملية", ex); }
            finally { IsBusy = false; }
        }
    }
}

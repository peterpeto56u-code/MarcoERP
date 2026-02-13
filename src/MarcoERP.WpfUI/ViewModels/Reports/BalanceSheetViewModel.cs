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
    public sealed class BalanceSheetViewModel : BaseViewModel
    {
        private readonly IReportService _reportService;
        private readonly IReportExportService _exportService;

        public BalanceSheetViewModel(IReportService reportService, IReportExportService exportService)
        {
            _reportService = reportService;
            _exportService = exportService;
            AsOfDate = DateTime.Today;
            AssetRows = new ObservableCollection<BalanceSheetRowDto>();
            LiabilityRows = new ObservableCollection<BalanceSheetRowDto>();
            EquityRows = new ObservableCollection<BalanceSheetRowDto>();
            GenerateCommand = new AsyncRelayCommand(GenerateAsync);
            ExportPdfCommand = new AsyncRelayCommand(ExportPdfAsync);
            ExportExcelCommand = new AsyncRelayCommand(ExportExcelAsync);
        }

        private DateTime _asOfDate;
        public DateTime AsOfDate { get => _asOfDate; set => SetProperty(ref _asOfDate, value); }

        public ObservableCollection<BalanceSheetRowDto> AssetRows { get; }
        public ObservableCollection<BalanceSheetRowDto> LiabilityRows { get; }
        public ObservableCollection<BalanceSheetRowDto> EquityRows { get; }

        private decimal _totalAssets;
        public decimal TotalAssets { get => _totalAssets; set => SetProperty(ref _totalAssets, value); }
        private decimal _totalLiabilities;
        public decimal TotalLiabilities { get => _totalLiabilities; set => SetProperty(ref _totalLiabilities, value); }
        private decimal _totalEquity;
        public decimal TotalEquity { get => _totalEquity; set => SetProperty(ref _totalEquity, value); }
        private decimal _retainedEarnings;
        public decimal RetainedEarnings { get => _retainedEarnings; set => SetProperty(ref _retainedEarnings, value); }
        private decimal _totalLiabilitiesAndEquity;
        public decimal TotalLiabilitiesAndEquity { get => _totalLiabilitiesAndEquity; set => SetProperty(ref _totalLiabilitiesAndEquity, value); }

        public ICommand GenerateCommand { get; }
        public ICommand ExportPdfCommand { get; }
        public ICommand ExportExcelCommand { get; }

        private ReportExportRequest BuildExportRequest()
        {
            var columns = new List<ReportColumn>
            {
                new("كود الحساب", 1f),
                new("اسم الحساب", 2f),
                new("النوع", 1f),
                new("الرصيد", 1.2f, true),
            };

            var req = new ReportExportRequest
            {
                Title = "الميزانية العمومية",
                Subtitle = $"بتاريخ {AsOfDate:yyyy/MM/dd}",
                Columns = columns,
                FooterSummary = $"إجمالي الأصول: {TotalAssets:N2}  |  إجمالي الالتزامات وحقوق الملكية: {TotalLiabilitiesAndEquity:N2}"
            };

            req.Rows.Add(new List<string> { "──", "── الأصول ──", "", "" });
            foreach (var r in AssetRows)
                req.Rows.Add(new List<string> { r.AccountCode, r.AccountNameAr, r.AccountTypeName, r.Balance.ToString("N2") });

            req.Rows.Add(new List<string> { "──", "── الالتزامات ──", "", "" });
            foreach (var r in LiabilityRows)
                req.Rows.Add(new List<string> { r.AccountCode, r.AccountNameAr, r.AccountTypeName, r.Balance.ToString("N2") });

            req.Rows.Add(new List<string> { "──", "── حقوق الملكية ──", "", "" });
            foreach (var r in EquityRows)
                req.Rows.Add(new List<string> { r.AccountCode, r.AccountNameAr, r.AccountTypeName, r.Balance.ToString("N2") });

            return req;
        }

        private async Task ExportPdfAsync()
        {
            if (AssetRows.Count == 0 && LiabilityRows.Count == 0 && EquityRows.Count == 0) { ErrorMessage = "لا توجد بيانات للتصدير."; return; }
            var result = await ReportExportHelper.ExportPdfAsync(_exportService, BuildExportRequest());
            if (result != null) StatusMessage = result.EndsWith(".pdf") ? "تم التصدير بنجاح" : result;
        }

        private async Task ExportExcelAsync()
        {
            if (AssetRows.Count == 0 && LiabilityRows.Count == 0 && EquityRows.Count == 0) { ErrorMessage = "لا توجد بيانات للتصدير."; return; }
            var result = await ReportExportHelper.ExportExcelAsync(_exportService, BuildExportRequest());
            if (result != null) StatusMessage = result.EndsWith(".xlsx") ? "تم التصدير بنجاح" : result;
        }

        private async Task GenerateAsync()
        {
            if (IsBusy) return;
            IsBusy = true; ClearError();
            try
            {
                var result = await _reportService.GetBalanceSheetAsync(AsOfDate);
                AssetRows.Clear(); LiabilityRows.Clear(); EquityRows.Clear();
                if (result.IsSuccess && result.Data != null)
                {
                    var d = result.Data;
                    foreach (var r in d.AssetRows) AssetRows.Add(r);
                    foreach (var r in d.LiabilityRows) LiabilityRows.Add(r);
                    foreach (var r in d.EquityRows) EquityRows.Add(r);
                    TotalAssets = d.TotalAssets;
                    TotalLiabilities = d.TotalLiabilities;
                    TotalEquity = d.TotalEquity;
                    RetainedEarnings = d.RetainedEarnings;
                    TotalLiabilitiesAndEquity = d.TotalLiabilitiesAndEquity;
                    StatusMessage = "تم إنشاء الميزانية العمومية";
                }
                else ErrorMessage = result.ErrorMessage ?? "فشل إنشاء التقرير.";
            }
            catch (Exception ex) { ErrorMessage = FriendlyErrorMessage("العملية", ex); }
            finally { IsBusy = false; }
        }
    }
}

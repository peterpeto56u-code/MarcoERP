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
    public sealed class IncomeStatementViewModel : BaseViewModel
    {
        private readonly IReportService _reportService;
        private readonly IReportExportService _exportService;

        public IncomeStatementViewModel(IReportService reportService, IReportExportService exportService)
        {
            _reportService = reportService;
            _exportService = exportService;
            FromDate = new DateTime(DateTime.Today.Year, 1, 1);
            ToDate = DateTime.Today;
            RevenueRows = new ObservableCollection<IncomeStatementRowDto>();
            CogsRows = new ObservableCollection<IncomeStatementRowDto>();
            ExpenseRows = new ObservableCollection<IncomeStatementRowDto>();
            OtherIncomeRows = new ObservableCollection<IncomeStatementRowDto>();
            OtherExpenseRows = new ObservableCollection<IncomeStatementRowDto>();
            GenerateCommand = new AsyncRelayCommand(GenerateAsync);
            ExportPdfCommand = new AsyncRelayCommand(ExportPdfAsync);
            ExportExcelCommand = new AsyncRelayCommand(ExportExcelAsync);
        }

        private DateTime _fromDate;
        public DateTime FromDate { get => _fromDate; set => SetProperty(ref _fromDate, value); }
        private DateTime _toDate;
        public DateTime ToDate { get => _toDate; set => SetProperty(ref _toDate, value); }

        public ObservableCollection<IncomeStatementRowDto> RevenueRows { get; }
        public ObservableCollection<IncomeStatementRowDto> CogsRows { get; }
        public ObservableCollection<IncomeStatementRowDto> ExpenseRows { get; }
        public ObservableCollection<IncomeStatementRowDto> OtherIncomeRows { get; }
        public ObservableCollection<IncomeStatementRowDto> OtherExpenseRows { get; }

        private decimal _totalRevenue;
        public decimal TotalRevenue { get => _totalRevenue; set => SetProperty(ref _totalRevenue, value); }
        private decimal _totalCogs;
        public decimal TotalCogs { get => _totalCogs; set => SetProperty(ref _totalCogs, value); }
        private decimal _grossProfit;
        public decimal GrossProfit { get => _grossProfit; set => SetProperty(ref _grossProfit, value); }
        private decimal _totalExpenses;
        public decimal TotalExpenses { get => _totalExpenses; set => SetProperty(ref _totalExpenses, value); }
        private decimal _totalOtherIncome;
        public decimal TotalOtherIncome { get => _totalOtherIncome; set => SetProperty(ref _totalOtherIncome, value); }
        private decimal _totalOtherExpenses;
        public decimal TotalOtherExpenses { get => _totalOtherExpenses; set => SetProperty(ref _totalOtherExpenses, value); }
        private decimal _netProfit;
        public decimal NetProfit { get => _netProfit; set => SetProperty(ref _netProfit, value); }

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
                new("المبلغ", 1.2f, true),
            };

            var req = new ReportExportRequest
            {
                Title = "قائمة الدخل",
                Subtitle = $"من {FromDate:yyyy/MM/dd} إلى {ToDate:yyyy/MM/dd}",
                Columns = columns,
                FooterSummary = $"إجمالي الإيرادات: {TotalRevenue:N2}  |  مجمل الربح: {GrossProfit:N2}  |  صافي الربح: {NetProfit:N2}"
            };

            void AddSection(string title, IEnumerable<IncomeStatementRowDto> rows)
            {
                req.Rows.Add(new List<string> { "──", $"── {title} ──", "", "" });
                foreach (var r in rows)
                    req.Rows.Add(new List<string> { r.AccountCode, r.AccountNameAr, r.AccountTypeName, r.Amount.ToString("N2") });
            }

            AddSection("الإيرادات", RevenueRows);
            AddSection("تكلفة المبيعات", CogsRows);
            AddSection("المصروفات", ExpenseRows);
            AddSection("إيرادات أخرى", OtherIncomeRows);
            AddSection("مصروفات أخرى", OtherExpenseRows);

            return req;
        }

        private async Task ExportPdfAsync()
        {
            if (RevenueRows.Count == 0 && ExpenseRows.Count == 0) { ErrorMessage = "لا توجد بيانات للتصدير."; return; }
            var result = await ReportExportHelper.ExportPdfAsync(_exportService, BuildExportRequest());
            if (result != null) StatusMessage = result.EndsWith(".pdf") ? "تم التصدير بنجاح" : result;
        }

        private async Task ExportExcelAsync()
        {
            if (RevenueRows.Count == 0 && ExpenseRows.Count == 0) { ErrorMessage = "لا توجد بيانات للتصدير."; return; }
            var result = await ReportExportHelper.ExportExcelAsync(_exportService, BuildExportRequest());
            if (result != null) StatusMessage = result.EndsWith(".xlsx") ? "تم التصدير بنجاح" : result;
        }

        private async Task GenerateAsync()
        {
            if (IsBusy) return;
            IsBusy = true; ClearError();
            try
            {
                var result = await _reportService.GetIncomeStatementAsync(FromDate, ToDate);
                RevenueRows.Clear(); CogsRows.Clear(); ExpenseRows.Clear();
                OtherIncomeRows.Clear(); OtherExpenseRows.Clear();
                if (result.IsSuccess && result.Data != null)
                {
                    var d = result.Data;
                    foreach (var r in d.RevenueRows) RevenueRows.Add(r);
                    foreach (var r in d.CogsRows) CogsRows.Add(r);
                    foreach (var r in d.ExpenseRows) ExpenseRows.Add(r);
                    foreach (var r in d.OtherIncomeRows) OtherIncomeRows.Add(r);
                    foreach (var r in d.OtherExpenseRows) OtherExpenseRows.Add(r);
                    TotalRevenue = d.TotalRevenue;
                    TotalCogs = d.TotalCogs;
                    GrossProfit = d.GrossProfit;
                    TotalExpenses = d.TotalExpenses;
                    TotalOtherIncome = d.TotalOtherIncome;
                    TotalOtherExpenses = d.TotalOtherExpenses;
                    NetProfit = d.NetProfit;
                    StatusMessage = "تم إنشاء قائمة الدخل";
                }
                else ErrorMessage = result.ErrorMessage ?? "فشل إنشاء التقرير.";
            }
            catch (Exception ex) { ErrorMessage = FriendlyErrorMessage("العملية", ex); }
            finally { IsBusy = false; }
        }
    }
}

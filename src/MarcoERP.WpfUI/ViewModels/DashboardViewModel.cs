using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using MarcoERP.Application.DTOs.Reports;
using MarcoERP.Application.Interfaces.Reports;
using MarcoERP.WpfUI.Models;
using MarcoERP.WpfUI.Navigation;
using MarcoERP.WpfUI.Views.Shell;
using MaterialDesignThemes.Wpf;

namespace MarcoERP.WpfUI.ViewModels
{
    /// <summary>
    /// ViewModel for the main Dashboard view.
    /// Displays key business metrics and alerts.
    /// </summary>
    public sealed class DashboardViewModel : BaseViewModel
    {
        private readonly IReportService _reportService;
        private readonly INavigationService _navigationService;

        private static readonly string ShortcutsFilePath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dashboard_shortcuts.json");

        /// <summary>All navigable screens with their metadata.</summary>
        private static readonly List<(string Key, string Title, string IconKind)> AllScreens = new()
        {
            ("SalesInvoices",        "فواتير البيع",      "ReceiptLong"),
            ("SalesInvoiceDetail",   "فاتورة بيع جديدة",  "CashRegister"),
            ("SalesReturns",         "مرتجعات البيع",     "UndoVariant"),
            ("Customers",            "العملاء",           "AccountGroup"),
            ("PriceLists",           "قوائم الأسعار",     "TagMultiple"),
            ("SalesQuotations",      "عروض الأسعار",      "FormatListBulleted"),
            ("PurchaseInvoices",     "فواتير الشراء",     "CartArrowDown"),
            ("PurchaseInvoiceDetail","فاتورة شراء جديدة", "CartOutline"),
            ("PurchaseReturns",      "مرتجعات الشراء",    "CartRemove"),
            ("Suppliers",            "الموردين",          "TruckDeliveryOutline"),
            ("PurchaseQuotations",   "طلبات الشراء",      "ClipboardTextOutline"),
            ("Products",             "الأصناف",           "PackageVariant"),
            ("Categories",           "التصنيفات",         "Shape"),
            ("Warehouses",           "المخازن",           "Warehouse"),
            ("InventoryAdjustments", "تسويات المخزون",    "PackageVariantClosedCheck"),
            ("CashReceipts",         "سندات القبض",       "CashPlus"),
            ("CashPayments",         "سندات الصرف",       "CashMinus"),
            ("CashTransfers",        "التحويلات",         "SwapHorizontal"),
            ("Cashboxes",            "الخزن",             "Safe"),
            ("BankAccounts",         "الحسابات البنكية",  "Bank"),
            ("ChartOfAccounts",      "شجرة الحسابات",     "FileTree"),
            ("JournalEntries",       "القيود اليومية",    "BookOpenPageVariant"),
            ("Reports",              "التقارير",          "ChartBar"),
            ("TrialBalance",         "ميزان المراجعة",    "Scale"),
            ("AccountStatement",     "كشف حساب",          "FileDocumentOutline"),
        };

        /// <summary>Default icon colors mapped by ViewKey for visual distinction.</summary>
        private static readonly Dictionary<string, string> DefaultColorMap = new()
        {
            ["SalesInvoiceDetail"]   = "#4CAF50", // Green
            ["PurchaseInvoiceDetail"]= "#FF9800", // Amber
            ["CashReceipts"]         = "#2196F3", // Blue
            ["CashPayments"]         = "#F44336", // Red
            ["Products"]             = "#FFC107", // Yellow
            ["Customers"]            = "#9C27B0", // Purple
        };

        private static readonly List<DashboardShortcut> DefaultShortcuts = new()
        {
            new() { ViewKey = "SalesInvoiceDetail",    Title = "فاتورة بيع",  IconKind = "CashRegister"  },
            new() { ViewKey = "PurchaseInvoiceDetail",  Title = "فاتورة شراء", IconKind = "CartOutline"   },
            new() { ViewKey = "CashReceipts",           Title = "سندات القبض", IconKind = "CashPlus"      },
            new() { ViewKey = "CashPayments",           Title = "سندات الصرف", IconKind = "CashMinus"     },
            new() { ViewKey = "Products",               Title = "الأصناف",     IconKind = "PackageVariant" },
            new() { ViewKey = "Customers",              Title = "العملاء",     IconKind = "AccountGroup"  },
        };

        public DashboardViewModel(IReportService reportService, INavigationService navigationService)
        {
            _reportService = reportService;
            _navigationService = navigationService;

            RefreshCommand = new AsyncRelayCommand(LoadDataAsync);
            ConfigureShortcutsCommand = new RelayCommand(ShowConfigureShortcutsDialog);

            LoadShortcuts();
            EnqueueDbWork(LoadDataAsync);
        }

        // ── Shortcut Items ──
        public ObservableCollection<DashboardShortcutItem> Shortcuts { get; } = new();

        public ICommand ConfigureShortcutsCommand { get; }

        // ── Shortcut persistence ──

        private void LoadShortcuts()
        {
            List<DashboardShortcut> saved;
            try
            {
                if (File.Exists(ShortcutsFilePath))
                {
                    var json = File.ReadAllText(ShortcutsFilePath);
                    saved = JsonSerializer.Deserialize<List<DashboardShortcut>>(json) ?? DefaultShortcuts;
                }
                else
                {
                    saved = DefaultShortcuts;
                }
            }
            catch
            {
                saved = DefaultShortcuts;
            }

            RebuildShortcutItems(saved);
        }

        private void SaveShortcuts(List<DashboardShortcut> shortcuts)
        {
            try
            {
                var json = JsonSerializer.Serialize(shortcuts, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(ShortcutsFilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DashboardVM] Failed to save shortcuts: {ex.Message}");
            }
        }

        private void RebuildShortcutItems(List<DashboardShortcut> shortcuts)
        {
            Shortcuts.Clear();
            foreach (var sc in shortcuts)
            {
                var iconKind = PackIconKind.OpenInNew;
                if (Enum.TryParse<PackIconKind>(sc.IconKind, true, out var parsed))
                    iconKind = parsed;

                var brush = GetBrushForKey(sc.ViewKey);
                var viewKey = sc.ViewKey;

                Shortcuts.Add(new DashboardShortcutItem
                {
                    Title = sc.Title,
                    IconKind = iconKind,
                    IconBrush = brush,
                    NavigateCommand = new RelayCommand(() => _navigationService.NavigateTo(viewKey))
                });
            }
        }

        private static SolidColorBrush GetBrushForKey(string viewKey)
        {
            if (DefaultColorMap.TryGetValue(viewKey, out var hex))
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));

            // Fallback: hash-based color for any screen
            var hash = Math.Abs(viewKey.GetHashCode());
            var fallbackColors = new[] { "#4CAF50", "#2196F3", "#FF9800", "#9C27B0", "#009688", "#795548" };
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString(
                fallbackColors[hash % fallbackColors.Length]));
        }

        private void ShowConfigureShortcutsDialog()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                var currentKeys = Shortcuts.Select((s, i) =>
                {
                    // Reverse-lookup the ViewKey from the AllScreens list by title match
                    var match = AllScreens.FirstOrDefault(a => a.Title == s.Title
                        || DefaultShortcuts.Any(d => d.Title == s.Title && d.ViewKey == a.Key));
                    return match.Key;
                }).Where(k => k != null).ToList();

                // Also match by icon for accuracy
                var saved = LoadShortcutsFromFile();
                var savedKeys = saved.Select(s => s.ViewKey).ToList();

                var dialog = new ShortcutConfigDialog(AllScreens, savedKeys)
                {
                    Owner = System.Windows.Application.Current.MainWindow
                };

                if (dialog.ShowDialog() == true && dialog.SelectedKeys.Count > 0)
                {
                    var newShortcuts = dialog.SelectedKeys
                        .Take(6)
                        .Select(key =>
                        {
                            var screen = AllScreens.FirstOrDefault(s => s.Key == key);
                            return new DashboardShortcut
                            {
                                ViewKey = key,
                                Title = screen.Title ?? key,
                                IconKind = screen.IconKind ?? "OpenInNew"
                            };
                        })
                        .ToList();

                    SaveShortcuts(newShortcuts);
                    RebuildShortcutItems(newShortcuts);
                }
            });
        }

        private List<DashboardShortcut> LoadShortcutsFromFile()
        {
            try
            {
                if (File.Exists(ShortcutsFilePath))
                {
                    var json = File.ReadAllText(ShortcutsFilePath);
                    return JsonSerializer.Deserialize<List<DashboardShortcut>>(json) ?? DefaultShortcuts;
                }
            }
            catch { }
            return DefaultShortcuts;
        }

        // ── Commands ──
        public ICommand RefreshCommand { get; }

        // ── Today ──
        private decimal _todaySales;
        public decimal TodaySales { get => _todaySales; set => SetProperty(ref _todaySales, value); }

        private decimal _todayPurchases;
        public decimal TodayPurchases { get => _todayPurchases; set => SetProperty(ref _todayPurchases, value); }

        private decimal _todayReceipts;
        public decimal TodayReceipts { get => _todayReceipts; set => SetProperty(ref _todayReceipts, value); }

        private decimal _todayPayments;
        public decimal TodayPayments { get => _todayPayments; set => SetProperty(ref _todayPayments, value); }

        private int _todaySalesCount;
        public int TodaySalesCount { get => _todaySalesCount; set => SetProperty(ref _todaySalesCount, value); }

        private int _todayPurchasesCount;
        public int TodayPurchasesCount { get => _todayPurchasesCount; set => SetProperty(ref _todayPurchasesCount, value); }

        // ── Month ──
        private decimal _monthSales;
        public decimal MonthSales { get => _monthSales; set => SetProperty(ref _monthSales, value); }

        private decimal _monthPurchases;
        public decimal MonthPurchases { get => _monthPurchases; set => SetProperty(ref _monthPurchases, value); }

        private decimal _monthReceipts;
        public decimal MonthReceipts { get => _monthReceipts; set => SetProperty(ref _monthReceipts, value); }

        private decimal _monthPayments;
        public decimal MonthPayments { get => _monthPayments; set => SetProperty(ref _monthPayments, value); }

        // ── Alerts ──
        private int _lowStockCount;
        public int LowStockCount { get => _lowStockCount; set => SetProperty(ref _lowStockCount, value); }

        private int _totalProducts;
        public int TotalProducts { get => _totalProducts; set => SetProperty(ref _totalProducts, value); }

        private int _pendingSalesInvoices;
        public int PendingSalesInvoices { get => _pendingSalesInvoices; set => SetProperty(ref _pendingSalesInvoices, value); }

        private int _pendingPurchaseInvoices;
        public int PendingPurchaseInvoices { get => _pendingPurchaseInvoices; set => SetProperty(ref _pendingPurchaseInvoices, value); }

        private int _pendingJournalEntries;
        public int PendingJournalEntries { get => _pendingJournalEntries; set => SetProperty(ref _pendingJournalEntries, value); }

        // ── Running Balances ──
        private decimal _cashBalance;
        public decimal CashBalance { get => _cashBalance; set => SetProperty(ref _cashBalance, value); }

        private decimal _totalCustomerBalance;
        public decimal TotalCustomerBalance { get => _totalCustomerBalance; set => SetProperty(ref _totalCustomerBalance, value); }

        private decimal _totalSupplierBalance;
        public decimal TotalSupplierBalance { get => _totalSupplierBalance; set => SetProperty(ref _totalSupplierBalance, value); }

        private decimal _monthGrossProfit;
        public decimal MonthGrossProfit { get => _monthGrossProfit; set => SetProperty(ref _monthGrossProfit, value); }

        private async Task LoadDataAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            ClearError();

            try
            {
                var result = await _reportService.GetDashboardSummaryAsync();
                if (result.IsSuccess && result.Data != null)
                {
                    var d = result.Data;
                    TodaySales = d.TodaySales;
                    TodayPurchases = d.TodayPurchases;
                    TodayReceipts = d.TodayReceipts;
                    TodayPayments = d.TodayPayments;
                    TodaySalesCount = d.TodaySalesCount;
                    TodayPurchasesCount = d.TodayPurchasesCount;
                    MonthSales = d.MonthSales;
                    MonthPurchases = d.MonthPurchases;
                    MonthReceipts = d.MonthReceipts;
                    MonthPayments = d.MonthPayments;
                    LowStockCount = d.LowStockCount;
                    TotalProducts = d.TotalProducts;
                    PendingSalesInvoices = d.PendingSalesInvoices;
                    PendingPurchaseInvoices = d.PendingPurchaseInvoices;
                    PendingJournalEntries = d.PendingJournalEntries;
                    CashBalance = d.CashBalance;
                    TotalCustomerBalance = d.TotalCustomerBalance;
                    TotalSupplierBalance = d.TotalSupplierBalance;
                    MonthGrossProfit = d.MonthGrossProfit;
                    StatusMessage = "تم تحديث البيانات";
                }
                else
                {
                    ErrorMessage = result.ErrorMessage ?? "فشل تحميل بيانات لوحة التحكم.";
                }
            }
            catch (System.Exception ex)
            {
                ErrorMessage = FriendlyErrorMessage("العملية", ex);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }

    /// <summary>
    /// Display item for a dashboard shortcut card, including the resolved icon and command.
    /// </summary>
    public sealed class DashboardShortcutItem
    {
        public string Title { get; set; } = string.Empty;
        public PackIconKind IconKind { get; set; }
        public Brush IconBrush { get; set; } = Brushes.Gray;
        public ICommand NavigateCommand { get; set; }
    }
}

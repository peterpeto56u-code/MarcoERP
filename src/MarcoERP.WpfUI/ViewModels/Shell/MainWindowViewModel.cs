using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using MaterialDesignThemes.Wpf;
using Microsoft.Extensions.DependencyInjection;
using MarcoERP.Application.Common;
using MarcoERP.Application.Interfaces;
using MarcoERP.Application.Interfaces.Search;
using MarcoERP.Application.Interfaces.Settings;
using MarcoERP.Application.DTOs.Search;
using MarcoERP.WpfUI.Navigation;
using MarcoERP.WpfUI.Services;

namespace MarcoERP.WpfUI.ViewModels.Shell
{
    public sealed class MainWindowViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IWindowService _windowService;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly DispatcherTimer _clockTimer;
        private readonly IServiceProvider _serviceProvider;
        private readonly DispatcherTimer _commandPaletteDebounceTimer;

        public MainWindowViewModel(
            INavigationService navigationService,
            ICurrentUserService currentUserService,
            IWindowService windowService,
            IDateTimeProvider dateTimeProvider,
            IServiceProvider serviceProvider)
        {
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _windowService = windowService ?? throw new ArgumentNullException(nameof(windowService));
            _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

            NavigationItems = new ObservableCollection<NavigationItem>();

            ToggleSidebarCommand = new RelayCommand(ToggleSidebar);
            LogoutCommand = new RelayCommand(Logout);
            ToggleThemeCommand = new RelayCommand(ToggleTheme);
            ShowNotificationsCommand = new RelayCommand(ShowNotifications);
            OpenCommandPaletteCommand = new RelayCommand(ToggleCommandPalette);
            ExecuteCommandPaletteItemCommand = new RelayCommand(ExecuteCommandPaletteItem);
            ExecuteShortcutCommand = new RelayCommand(ExecuteShortcut);
            ActivateTabCommand = new AsyncRelayCommand(ActivateTabAsync);
            CloseTabCommand = new AsyncRelayCommand(CloseTabAsync);
            ActivateNextTabCommand = new AsyncRelayCommand(ActivateNextTabAsync);
            ActivatePreviousTabCommand = new AsyncRelayCommand(ActivatePreviousTabAsync);
            CloseActiveTabCommand = new AsyncRelayCommand(CloseActiveTabAsync);
            CloseOtherTabsCommand = new AsyncRelayCommand(CloseOtherTabsAsync);
            CloseAllTabsCommand = new AsyncRelayCommand(CloseAllTabsAsync);

            _navigationService.NavigationChanged += OnNavigationChanged;

            BuildNavigationItems();

            // Load feature flags asynchronously and refresh navigation
            _ = RefreshNavigationAsync();

            // Load notifications on startup
            _ = LoadNotificationsAsync();

            StatusMessage = "جاهز";

            _clockTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
            _clockTimer.Tick += (_, _) => UpdateDateTime();
            _clockTimer.Start();
            UpdateDateTime();

            _commandPaletteDebounceTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(250) };
            _commandPaletteDebounceTimer.Tick += async (_, _) =>
            {
                _commandPaletteDebounceTimer.Stop();
                await RefreshCommandPaletteItemsAsync();
            };

            var initial = NavigationItems.FirstOrDefault(i => i.ItemType == NavigationItemType.Item);
            if (initial != null)
                Navigate(initial);
        }

        public ObservableCollection<NavigationItem> NavigationItems { get; }

        public ObservableCollection<DocumentTab> OpenTabs { get; } = new();

        private NavigationItem _activeItem;

        private DocumentTab _activeTab;
        public DocumentTab ActiveTab
        {
            get => _activeTab;
            set
            {
                if (SetProperty(ref _activeTab, value))
                {
                    foreach (var tab in OpenTabs)
                        tab.IsActive = tab == _activeTab;

                    PageTitle = _activeTab?.Title ?? "لوحة التحكم";
                    CurrentView = _activeTab?.View;
                }
            }
        }

        private string _pageTitle = "لوحة التحكم";
        public string PageTitle
        {
            get => _pageTitle;
            set => SetProperty(ref _pageTitle, value);
        }

        private string _currentDateTime;
        public string CurrentDateTime
        {
            get => _currentDateTime;
            set => SetProperty(ref _currentDateTime, value);
        }

        public string CurrentUserDisplay => string.IsNullOrWhiteSpace(_currentUserService.FullNameAr)
            ? _currentUserService.Username
            : _currentUserService.FullNameAr;

        private bool _isSidebarExpanded = true;
        public bool IsSidebarExpanded
        {
            get => _isSidebarExpanded;
            set => SetProperty(ref _isSidebarExpanded, value);
        }

        private string _globalSearchText;
        public string GlobalSearchText
        {
            get => _globalSearchText;
            set => SetProperty(ref _globalSearchText, value);
        }

        private bool _isDarkTheme;
        public MaterialDesignThemes.Wpf.PackIconKind ThemeIcon => _isDarkTheme
            ? MaterialDesignThemes.Wpf.PackIconKind.WeatherSunny
            : MaterialDesignThemes.Wpf.PackIconKind.WeatherNight;

        private int _notificationCount;
        public int NotificationCount
        {
            get => _notificationCount;
            set
            {
                SetProperty(ref _notificationCount, value);
                OnPropertyChanged(nameof(HasNotifications));
            }
        }

        public bool HasNotifications => _notificationCount > 0;

        private bool _isCommandPaletteOpen;
        public bool IsCommandPaletteOpen
        {
            get => _isCommandPaletteOpen;
            set => SetProperty(ref _isCommandPaletteOpen, value);
        }

        private string _commandPaletteSearch;
        public string CommandPaletteSearch
        {
            get => _commandPaletteSearch;
            set
            {
                SetProperty(ref _commandPaletteSearch, value);
                ScheduleCommandPaletteRefresh();
            }
        }

        public ObservableCollection<NavigationItem> FilteredCommandItems { get; } = new();

        public ICommand ToggleSidebarCommand { get; }

        public ICommand LogoutCommand { get; }

        public ICommand ToggleThemeCommand { get; }

        public ICommand ShowNotificationsCommand { get; }

        public ICommand OpenCommandPaletteCommand { get; }

        public ICommand ExecuteCommandPaletteItemCommand { get; }

        public ICommand ExecuteShortcutCommand { get; }

        public ICommand ActivateTabCommand { get; }

        public ICommand CloseTabCommand { get; }

        public ICommand ActivateNextTabCommand { get; }

        public ICommand ActivatePreviousTabCommand { get; }

        public ICommand CloseActiveTabCommand { get; }

        public ICommand CloseOtherTabsCommand { get; }

        public ICommand CloseAllTabsCommand { get; }

        private System.Windows.Controls.UserControl _currentView;
        public System.Windows.Controls.UserControl CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        private void UpdateDateTime()
        {
            var now = _dateTimeProvider.UtcNow.ToLocalTime();
            CurrentDateTime = now.ToString("yyyy-MM-dd  hh:mm tt");
        }

        private void ToggleSidebar()
        {
            IsSidebarExpanded = !IsSidebarExpanded;
        }

        private void ToggleTheme()
        {
            _isDarkTheme = !_isDarkTheme;
            var paletteHelper = new MaterialDesignThemes.Wpf.PaletteHelper();
            var theme = paletteHelper.GetTheme();
            theme.SetBaseTheme(_isDarkTheme
                ? MaterialDesignThemes.Wpf.BaseTheme.Dark
                : MaterialDesignThemes.Wpf.BaseTheme.Light);
            paletteHelper.SetTheme(theme);
            OnPropertyChanged(nameof(ThemeIcon));
        }

        private void ShowNotifications()
        {
            IsNotificationPanelOpen = !IsNotificationPanelOpen;
            if (IsNotificationPanelOpen)
            {
                _ = LoadNotificationsAsync();
            }
        }

        private bool _isNotificationPanelOpen;
        public bool IsNotificationPanelOpen
        {
            get => _isNotificationPanelOpen;
            set => SetProperty(ref _isNotificationPanelOpen, value);
        }

        public ObservableCollection<NotificationItemViewModel> Notifications { get; } = new();

        private async Task LoadNotificationsAsync()
        {
            try
            {
                Notifications.Clear();
                using var scope = _serviceProvider.CreateScope();
                var auditLogService = scope.ServiceProvider.GetRequiredService<MarcoERP.Application.Interfaces.Settings.IAuditLogService>();

                var today = _dateTimeProvider.UtcNow.Date;
                var result = await auditLogService.GetByDateRangeAsync(today.AddDays(-7), today.AddDays(1));
                if (result.IsSuccess && result.Data?.Count > 0)
                {
                    await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        var items = result.Data.Take(20);
                        foreach (var log in items)
                        {
                            Notifications.Add(new NotificationItemViewModel
                            {
                                Title = GetNotificationTitle(log.Action, log.EntityType),
                                Detail = log.Details ?? string.Empty,
                                Timestamp = log.Timestamp.ToLocalTime().ToString("MM/dd HH:mm"),
                                Icon = GetNotificationIcon(log.Action),
                            });
                        }
                        NotificationCount = Notifications.Count;
                    });
                }
            }
            catch
            {
                // Best-effort notification loading
            }
        }

        private static string GetNotificationTitle(string action, string entityType)
        {
            var actionAr = action switch
            {
                "Created" => "تم إنشاء",
                "Updated" => "تم تعديل",
                "Deleted" => "تم حذف",
                "Posted" => "تم ترحيل",
                "Locked" => "تم قفل",
                "Unlocked" => "تم فتح",
                _ => action
            };

            var entityAr = entityType switch
            {
                "SalesInvoice" => "فاتورة بيع",
                "PurchaseInvoice" => "فاتورة شراء",
                "JournalEntry" => "قيد يومية",
                "Product" => "صنف",
                "Customer" => "عميل",
                "Supplier" => "مورد",
                "CashReceipt" => "سند قبض",
                "CashPayment" => "سند صرف",
                "FiscalYear" => "سنة مالية",
                "FiscalPeriod" => "فترة مالية",
                _ => entityType
            };

            return $"{actionAr} {entityAr}";
        }

        private static MaterialDesignThemes.Wpf.PackIconKind GetNotificationIcon(string action)
        {
            return action switch
            {
                "Created" => PackIconKind.PlusCircle,
                "Updated" => PackIconKind.Pencil,
                "Deleted" => PackIconKind.Delete,
                "Posted" => PackIconKind.CheckCircle,
                "Locked" => PackIconKind.Lock,
                "Unlocked" => PackIconKind.LockOpen,
                _ => PackIconKind.InformationOutline,
            };
        }

        private void ToggleCommandPalette()
        {
            IsCommandPaletteOpen = !IsCommandPaletteOpen;
            if (IsCommandPaletteOpen)
            {
                CommandPaletteSearch = string.Empty;
            }
        }

        private void ScheduleCommandPaletteRefresh()
        {
            if (!IsCommandPaletteOpen)
                return;

            _commandPaletteDebounceTimer.Stop();
            _commandPaletteDebounceTimer.Start();
        }

        private async Task RefreshCommandPaletteItemsAsync()
        {
            FilteredCommandItems.Clear();

            var query = _commandPaletteSearch?.Trim() ?? string.Empty;

            foreach (var item in NavigationItems.Where(i =>
                i.ItemType == NavigationItemType.Item && i.IsVisible &&
                (string.IsNullOrEmpty(query) || i.Title.Contains(query, StringComparison.OrdinalIgnoreCase))))
            {
                FilteredCommandItems.Add(item);
            }

            if (string.IsNullOrWhiteSpace(query))
                return;

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var searchService = scope.ServiceProvider.GetRequiredService<IGlobalSearchQueryService>();
                var hits = await searchService.SearchAsync(query);
                foreach (var hit in hits)
                {
                    var searchItem = CreateSearchItem(hit);
                    if (searchItem != null)
                        FilteredCommandItems.Add(searchItem);
                }
            }
            catch
            {
                // Search is best-effort; ignore transient failures
            }
        }

        private NavigationItem CreateSearchItem(GlobalSearchHitDto hit)
        {
            if (hit == null)
                return null;

            var (viewKey, supportsDirectOpen) = GetTargetView(hit.EntityType);
            if (string.IsNullOrWhiteSpace(viewKey))
                return null;

            var (iconKind, iconBrush) = GetViewIcon(viewKey);

            var titlePrefix = hit.EntityType switch
            {
                GlobalSearchEntityType.Customer => "عميل: ",
                GlobalSearchEntityType.Product => "صنف: ",
                GlobalSearchEntityType.SalesInvoice => "فاتورة بيع: ",
                GlobalSearchEntityType.PurchaseInvoice => "فاتورة شراء: ",
                GlobalSearchEntityType.JournalEntry => "قيد: ",
                GlobalSearchEntityType.CashReceipt => "سند قبض: ",
                GlobalSearchEntityType.CashPayment => "سند صرف: ",
                GlobalSearchEntityType.Supplier => "مورد: ",
                _ => "نتيجة: ",
            };

            var item = new NavigationItem(NavigationItemType.Item, titlePrefix + (hit.Title ?? string.Empty))
            {
                IconKind = iconKind,
                IconBrush = iconBrush,
            };

            item.Command = new RelayCommand(_ =>
            {
                if (supportsDirectOpen)
                    _navigationService.NavigateTo(viewKey, hit.Id);
                else
                    _navigationService.NavigateTo(viewKey);
            });

            return item;
        }

        private (string viewKey, bool supportsDirectOpen) GetTargetView(GlobalSearchEntityType entityType)
        {
            return entityType switch
            {
                GlobalSearchEntityType.Customer => ("Customers", false),
                GlobalSearchEntityType.Product => ("Products", false),
                GlobalSearchEntityType.SalesInvoice => ("SalesInvoiceDetail", true),
                GlobalSearchEntityType.PurchaseInvoice => ("PurchaseInvoiceDetail", true),
                GlobalSearchEntityType.JournalEntry => ("JournalEntries", false),
                GlobalSearchEntityType.CashReceipt => ("CashReceipts", false),
                GlobalSearchEntityType.CashPayment => ("CashPayments", false),
                GlobalSearchEntityType.Supplier => ("Suppliers", false),
                _ => (null, false),
            };
        }

        private (MaterialDesignThemes.Wpf.PackIconKind kind, Brush brush) GetViewIcon(string viewKey)
        {
            var navItem = NavigationItems.FirstOrDefault(i =>
                i.ItemType == NavigationItemType.Item &&
                string.Equals(i.ViewKey, viewKey, StringComparison.OrdinalIgnoreCase));

            if (navItem != null)
                return (navItem.IconKind, navItem.IconBrush);

            return (MaterialDesignThemes.Wpf.PackIconKind.Magnify, Brushes.Gray);
        }

        private void ExecuteCommandPaletteItem(object param)
        {
            if (param is NavigationItem item)
            {
                IsCommandPaletteOpen = false;

                if (item.Command != null)
                    item.Command.Execute(null);
                else
                    Navigate(item);
            }
        }

        private void Logout()
        {
            _clockTimer.Stop();
            _navigationService.NavigationChanged -= OnNavigationChanged;
            _windowService.LogoutToLogin();
        }

        private void ExecuteShortcut(object param)
        {
            var action = param as string;
            if (string.IsNullOrWhiteSpace(action))
                return;

            _ = action switch
            {
                "New" => TryExecuteActiveViewModelCommand("NewCommand"),
                "Save" => TryExecuteActiveViewModelCommand("SaveCommand"),
                "Edit" => TryExecuteActiveViewModelCommand("EditCommand", "EditSelectedCommand"),
                "Refresh" => TryExecuteActiveViewModelCommand("LoadCommand"),
                "Print" => TryExecuteActiveViewModelCommand("PrintCommand"),
                "Post" => TryExecuteActiveViewModelCommand("PostCommand"),
                "Cancel" => TryExecuteActiveViewModelCommand("CancelEditCommand"),
                "Next" => TryExecuteActiveViewModelCommand("GoToNextCommand"),
                "Previous" => TryExecuteActiveViewModelCommand("GoToPreviousCommand"),
                _ => false
            };
        }

        private bool TryExecuteActiveViewModelCommand(params string[] commandNames)
        {
            var vm = ActiveTab?.View?.DataContext ?? CurrentView?.DataContext;
            if (vm == null)
                return false;

            var type = vm.GetType();
            foreach (var name in commandNames)
            {
                var prop = type.GetProperty(name);
                if (prop?.GetValue(vm) is ICommand cmd && cmd.CanExecute(null))
                {
                    cmd.Execute(null);
                    return true;
                }
            }

            return false;
        }

        private void OnNavigationChanged(object sender, NavigationChangedEventArgs e)
        {
            SyncActiveItemByViewKey(e?.Key);
            OpenOrActivateTab(e);
            StatusMessage = CurrentView != null
                ? $"تم فتح: {e.Title}"
                : $"تعذر فتح: {e?.Title}";
        }

        private void SyncActiveItemByViewKey(string viewKey)
        {
            if (string.IsNullOrWhiteSpace(viewKey))
                return;

            var match = NavigationItems.FirstOrDefault(i =>
                i.ItemType == NavigationItemType.Item &&
                !string.IsNullOrWhiteSpace(i.ViewKey) &&
                string.Equals(i.ViewKey, viewKey, StringComparison.OrdinalIgnoreCase));

            if (match != null)
                SetActiveItem(match);
        }

        private void OpenOrActivateTab(NavigationChangedEventArgs e)
        {
            if (e == null) return;

            var tabKey = BuildTabKey(e.Key, e.Parameter);
            var existing = OpenTabs.FirstOrDefault(t => string.Equals(t.TabKey, tabKey, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                ActiveTab = existing;
                return;
            }

            // Look up icon from the matching sidebar navigation item
            var navItem = NavigationItems.FirstOrDefault(i =>
                i.ItemType == NavigationItemType.Item &&
                !string.IsNullOrWhiteSpace(i.ViewKey) &&
                string.Equals(i.ViewKey, e.Key, StringComparison.OrdinalIgnoreCase));

            var tab = new DocumentTab(tabKey, e.Title, e.View, e.Parameter)
            {
                ViewKey = e.Key,
                IconKind = navItem?.IconKind ?? MaterialDesignThemes.Wpf.PackIconKind.FileDocumentOutline,
                IconBrush = navItem?.IconBrush
            };

            OpenTabs.Add(tab);
            ActiveTab = tab;
        }

        private async Task ActivateTabAsync(object parameter)
        {
            if (parameter is not DocumentTab tab) return;
            if (ActiveTab == tab) return;

            if (ActiveTab?.View?.DataContext is IDirtyStateAware dirty)
            {
                var canContinue = await DirtyStateGuard.ConfirmContinueAsync(dirty);
                if (!canContinue)
                    return;
            }

            ActiveTab = tab;
        }

        private async Task CloseTabAsync(object parameter)
        {
            if (parameter is not DocumentTab tab) return;

            if (tab.View?.DataContext is IDirtyStateAware dirty)
            {
                var canClose = await DirtyStateGuard.ConfirmContinueAsync(dirty);
                if (!canClose)
                    return;
            }

            var wasActive = ActiveTab == tab;
            tab.UnhookDirtyTracking();
            OpenTabs.Remove(tab);
            _navigationService.CloseView(tab.ViewKey, tab.Parameter);

            if (wasActive)
            {
                ActiveTab = OpenTabs.LastOrDefault();
                if (ActiveTab == null)
                    PageTitle = "لوحة التحكم";
            }
        }

        private async Task ActivateNextTabAsync()
        {
            if (OpenTabs.Count == 0 || ActiveTab == null)
                return;

            var index = OpenTabs.IndexOf(ActiveTab);
            if (index < 0) return;

            var nextIndex = (index + 1) % OpenTabs.Count;
            await ActivateTabAsync(OpenTabs[nextIndex]);
        }

        private async Task ActivatePreviousTabAsync()
        {
            if (OpenTabs.Count == 0 || ActiveTab == null)
                return;

            var index = OpenTabs.IndexOf(ActiveTab);
            if (index < 0) return;

            var prevIndex = index - 1;
            if (prevIndex < 0) prevIndex = OpenTabs.Count - 1;

            await ActivateTabAsync(OpenTabs[prevIndex]);
        }

        private async Task CloseActiveTabAsync()
        {
            if (ActiveTab == null)
                return;

            await CloseTabAsync(ActiveTab);
        }

        private async Task CloseOtherTabsAsync(object parameter)
        {
            var keepTab = parameter as DocumentTab ?? ActiveTab;
            if (keepTab == null) return;

            var tabsToClose = OpenTabs.Where(t => t != keepTab).ToList();
            foreach (var tab in tabsToClose)
            {
                if (tab.View?.DataContext is IDirtyStateAware dirty)
                {
                    var canClose = await DirtyStateGuard.ConfirmContinueAsync(dirty);
                    if (!canClose) return;
                }

                tab.UnhookDirtyTracking();
                OpenTabs.Remove(tab);
                _navigationService.CloseView(tab.ViewKey, tab.Parameter);
            }

            ActiveTab = keepTab;
        }

        private async Task CloseAllTabsAsync()
        {
            var tabsToClose = OpenTabs.ToList();
            foreach (var tab in tabsToClose)
            {
                if (tab.View?.DataContext is IDirtyStateAware dirty)
                {
                    var canClose = await DirtyStateGuard.ConfirmContinueAsync(dirty);
                    if (!canClose) return;
                }

                tab.UnhookDirtyTracking();
                OpenTabs.Remove(tab);
                _navigationService.CloseView(tab.ViewKey, tab.Parameter);
            }

            ActiveTab = null;
            PageTitle = "لوحة التحكم";
        }

        private static string BuildTabKey(string key, object parameter)
        {
            if (parameter == null) return key;
            return key + ":" + parameter;
        }

        private void Navigate(NavigationItem item)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.ViewKey))
                return;

            SetActiveItem(item);
            _navigationService.NavigateTo(item.ViewKey);
        }

        private void SetActiveItem(NavigationItem item)
        {
            if (_activeItem == item) return;

            if (_activeItem != null)
                _activeItem.IsActive = false;

            _activeItem = item;
            _activeItem.IsActive = true;
        }

        private bool IsVisibleForUser(string permissionKey)
        {
            if (string.IsNullOrWhiteSpace(permissionKey)) return true;
            return _currentUserService.HasPermission(permissionKey);
        }

        private void AddSection(string title, params NavigationItem[] items)
        {
            var visibleItems = items.Where(i => i.IsVisible).ToArray();
            if (visibleItems.Length == 0) return;

            NavigationItems.Add(new NavigationItem(NavigationItemType.Separator) { IsVisible = true });
            NavigationItems.Add(new NavigationItem(NavigationItemType.Header, title) { IsVisible = true });

            foreach (var item in visibleItems)
                NavigationItems.Add(item);
        }

        private NavigationItem CreateItem(string title, string viewKey, MaterialDesignThemes.Wpf.PackIconKind iconKind, Brush iconBrush, string permissionKey = null)
        {
            var item = new NavigationItem(NavigationItemType.Item, title)
            {
                ViewKey = viewKey,
                IconKind = iconKind,
                IconBrush = iconBrush,
                PermissionKey = permissionKey
            };

            item.IsVisible = IsVisibleForUser(permissionKey);
            item.Command = new RelayCommand(_ => Navigate(item));
            return item;
        }

        private NavigationItem CreateCommandItem(string title, MaterialDesignThemes.Wpf.PackIconKind iconKind, Brush iconBrush, ICommand command, string permissionKey = null)
        {
            var item = new NavigationItem(NavigationItemType.Item, title)
            {
                IconKind = iconKind,
                IconBrush = iconBrush,
                PermissionKey = permissionKey,
                Command = command
            };

            item.IsVisible = IsVisibleForUser(permissionKey);
            return item;
        }

        private void BuildNavigationItems()
        {
            NavigationItems.Clear();

            var dashboard = CreateItem("لوحة التحكم", "Dashboard", MaterialDesignThemes.Wpf.PackIconKind.ViewDashboard, new SolidColorBrush(Color.FromRgb(144, 202, 249)));
            NavigationItems.Add(dashboard);

            AddSection("المحاسبة",
                CreateItem("شجرة الحسابات", "ChartOfAccounts", MaterialDesignThemes.Wpf.PackIconKind.FileTree, new SolidColorBrush(Color.FromRgb(165, 214, 167)), PermissionKeys.AccountsView),
                CreateItem("القيود اليومية", "JournalEntries", MaterialDesignThemes.Wpf.PackIconKind.BookOpenPageVariant, new SolidColorBrush(Color.FromRgb(165, 214, 167)), PermissionKeys.JournalView),
                CreateItem("الفترات المالية", "FiscalPeriods", MaterialDesignThemes.Wpf.PackIconKind.CalendarMonth, new SolidColorBrush(Color.FromRgb(165, 214, 167)), PermissionKeys.FiscalPeriodManage),
                CreateItem("الأرصدة الافتتاحية", "OpeningBalance", MaterialDesignThemes.Wpf.PackIconKind.ScaleBalance, new SolidColorBrush(Color.FromRgb(165, 214, 167)), PermissionKeys.AccountsView));

            AddSection("المخزون",
                CreateItem("التصنيفات", "Categories", MaterialDesignThemes.Wpf.PackIconKind.Shape, new SolidColorBrush(Color.FromRgb(255, 224, 130)), PermissionKeys.InventoryView),
                CreateItem("وحدات القياس", "Units", MaterialDesignThemes.Wpf.PackIconKind.Ruler, new SolidColorBrush(Color.FromRgb(255, 224, 130)), PermissionKeys.InventoryView),
                CreateItem("الأصناف", "Products", MaterialDesignThemes.Wpf.PackIconKind.PackageVariant, new SolidColorBrush(Color.FromRgb(255, 224, 130)), PermissionKeys.InventoryView),
                CreateItem("المخازن", "Warehouses", MaterialDesignThemes.Wpf.PackIconKind.Warehouse, new SolidColorBrush(Color.FromRgb(255, 224, 130)), PermissionKeys.InventoryView),
                CreateItem("تسويات المخزون", "InventoryAdjustments", MaterialDesignThemes.Wpf.PackIconKind.ClipboardCheck, new SolidColorBrush(Color.FromRgb(255, 224, 130)), PermissionKeys.InventoryAdjustmentView),
                CreateItem("تحديث الأسعار", "BulkPriceUpdate", MaterialDesignThemes.Wpf.PackIconKind.TagMultiple, new SolidColorBrush(Color.FromRgb(255, 224, 130)), PermissionKeys.InventoryManage),
                CreateItem("استيراد الأصناف", "ProductImport", MaterialDesignThemes.Wpf.PackIconKind.FileImport, new SolidColorBrush(Color.FromRgb(255, 224, 130)), PermissionKeys.InventoryManage));

            AddSection("المبيعات",
                CreateItem("فواتير البيع", "SalesInvoices", MaterialDesignThemes.Wpf.PackIconKind.ReceiptTextOutline, new SolidColorBrush(Color.FromRgb(239, 154, 154)), PermissionKeys.SalesView),
                CreateItem("مرتجعات البيع", "SalesReturns", MaterialDesignThemes.Wpf.PackIconKind.ReceiptTextMinus, new SolidColorBrush(Color.FromRgb(239, 154, 154)), PermissionKeys.SalesView),
                CreateItem("العملاء", "Customers", MaterialDesignThemes.Wpf.PackIconKind.AccountGroup, new SolidColorBrush(Color.FromRgb(239, 154, 154)), PermissionKeys.SalesView),
                CreateItem("مندوبي المبيعات", "SalesRepresentatives", MaterialDesignThemes.Wpf.PackIconKind.BadgeAccount, new SolidColorBrush(Color.FromRgb(239, 154, 154)), PermissionKeys.SalesView),
                CreateItem("قوائم الأسعار", "PriceLists", MaterialDesignThemes.Wpf.PackIconKind.CurrencyUsd, new SolidColorBrush(Color.FromRgb(239, 154, 154)), PermissionKeys.PriceListView),
                CreateItem("عروض الأسعار", "SalesQuotations", MaterialDesignThemes.Wpf.PackIconKind.FileDocumentEdit, new SolidColorBrush(Color.FromRgb(239, 154, 154)), PermissionKeys.SalesQuotationView),
                CreateCommandItem("نقطة البيع", MaterialDesignThemes.Wpf.PackIconKind.Store, new SolidColorBrush(Color.FromRgb(239, 154, 154)), new RelayCommand(_ => _windowService.OpenPosWindow())));

            AddSection("المشتريات",
                CreateItem("فواتير الشراء", "PurchaseInvoices", MaterialDesignThemes.Wpf.PackIconKind.CartOutline, new SolidColorBrush(Color.FromRgb(206, 147, 216)), PermissionKeys.PurchasesView),
                CreateItem("مرتجعات الشراء", "PurchaseReturns", MaterialDesignThemes.Wpf.PackIconKind.CartMinus, new SolidColorBrush(Color.FromRgb(206, 147, 216)), PermissionKeys.PurchasesView),
                CreateItem("الموردين", "Suppliers", MaterialDesignThemes.Wpf.PackIconKind.TruckDelivery, new SolidColorBrush(Color.FromRgb(206, 147, 216)), PermissionKeys.PurchasesView),
                CreateItem("طلبات الشراء", "PurchaseQuotations", MaterialDesignThemes.Wpf.PackIconKind.ClipboardTextSearch, new SolidColorBrush(Color.FromRgb(206, 147, 216)), PermissionKeys.PurchaseQuotationView));

            AddSection("الخزينة",
                CreateItem("الخزن", "Cashboxes", MaterialDesignThemes.Wpf.PackIconKind.Safe, new SolidColorBrush(Color.FromRgb(128, 203, 196)), PermissionKeys.TreasuryView),
                CreateItem("الحسابات البنكية", "BankAccounts", MaterialDesignThemes.Wpf.PackIconKind.Bank, new SolidColorBrush(Color.FromRgb(128, 203, 196)), PermissionKeys.TreasuryView),
                CreateItem("التسوية البنكية", "BankReconciliation", MaterialDesignThemes.Wpf.PackIconKind.ScaleBalance, new SolidColorBrush(Color.FromRgb(128, 203, 196)), PermissionKeys.TreasuryView),
                CreateItem("سندات القبض", "CashReceipts", MaterialDesignThemes.Wpf.PackIconKind.CashPlus, new SolidColorBrush(Color.FromRgb(128, 203, 196)), PermissionKeys.TreasuryView),
                CreateItem("سندات الصرف", "CashPayments", MaterialDesignThemes.Wpf.PackIconKind.CashMinus, new SolidColorBrush(Color.FromRgb(128, 203, 196)), PermissionKeys.TreasuryView),
                CreateItem("التحويلات", "CashTransfers", MaterialDesignThemes.Wpf.PackIconKind.SwapHorizontal, new SolidColorBrush(Color.FromRgb(128, 203, 196)), PermissionKeys.TreasuryView),
                CreateCommandItem("سند قبض سريع", MaterialDesignThemes.Wpf.PackIconKind.CashPlus, new SolidColorBrush(Color.FromRgb(128, 203, 196)), new RelayCommand(_ => OpenQuickCashReceipt()), PermissionKeys.TreasuryView),
                CreateCommandItem("سند صرف سريع", MaterialDesignThemes.Wpf.PackIconKind.CashMinus, new SolidColorBrush(Color.FromRgb(128, 203, 196)), new RelayCommand(_ => OpenQuickCashPayment()), PermissionKeys.TreasuryView));

            AddSection("التقارير",
                CreateItem("التقارير", "Reports", MaterialDesignThemes.Wpf.PackIconKind.ChartBar, new SolidColorBrush(Color.FromRgb(176, 190, 197)), PermissionKeys.ReportsView));

            AddSection("الإعدادات",
                CreateItem("السنة المالية", "FiscalYear", MaterialDesignThemes.Wpf.PackIconKind.Calendar, new SolidColorBrush(Color.FromRgb(176, 190, 197))),
                CreateItem("إعدادات النظام", "SystemSettings", MaterialDesignThemes.Wpf.PackIconKind.Cog, new SolidColorBrush(Color.FromRgb(176, 190, 197)), PermissionKeys.SettingsManage),
                CreateItem("إدارة المستخدمين", "UserManagement", MaterialDesignThemes.Wpf.PackIconKind.AccountMultiple, new SolidColorBrush(Color.FromRgb(176, 190, 197)), PermissionKeys.UsersManage),
                CreateItem("إدارة الأدوار", "RoleManagement", MaterialDesignThemes.Wpf.PackIconKind.ShieldAccount, new SolidColorBrush(Color.FromRgb(176, 190, 197)), PermissionKeys.RolesManage),
                CreateItem("سجل المراجعة", "AuditLog", MaterialDesignThemes.Wpf.PackIconKind.ClipboardTextClock, new SolidColorBrush(Color.FromRgb(176, 190, 197)), PermissionKeys.AuditLogView),
                CreateItem("النسخ الاحتياطي", "BackupSettings", MaterialDesignThemes.Wpf.PackIconKind.DatabaseExport, new SolidColorBrush(Color.FromRgb(176, 190, 197))),
                CreateItem("فحص السلامة", "IntegrityCheck", MaterialDesignThemes.Wpf.PackIconKind.ShieldCheck, new SolidColorBrush(Color.FromRgb(176, 190, 197))),
                // Phase 7G: GovernanceConsole, GovernanceIntegrity, MigrationCenter removed from sidebar
                // They are accessible only via the hidden governance trigger (7B)
                CreateCommandItem("تسجيل الخروج", MaterialDesignThemes.Wpf.PackIconKind.Logout, new SolidColorBrush(Color.FromRgb(239, 83, 80)), LogoutCommand));
        }

        // ── Quick Treasury Dialogs ────────────────────────────

        private void OpenQuickCashReceipt()
        {
            using var scope = _serviceProvider.CreateScope();
            var window = scope.ServiceProvider.GetRequiredService<Views.Treasury.QuickCashReceiptWindow>();
            if (System.Windows.Application.Current?.MainWindow != null)
                window.Owner = System.Windows.Application.Current.MainWindow;
            window.ShowDialog();
        }

        private void OpenQuickCashPayment()
        {
            using var scope = _serviceProvider.CreateScope();
            var window = scope.ServiceProvider.GetRequiredService<Views.Treasury.QuickCashPaymentWindow>();
            if (System.Windows.Application.Current?.MainWindow != null)
                window.Owner = System.Windows.Application.Current.MainWindow;
            window.ShowDialog();
        }

        // ── Section → FeatureKey mapping ────────────────────────
        // Maps sidebar section titles to their corresponding Feature keys.
        // Sections not in this dictionary are always visible (Dashboard, Settings).
        private static readonly System.Collections.Generic.Dictionary<string, string> _sectionFeatureMap = new(StringComparer.OrdinalIgnoreCase)
        {
            { "المحاسبة",   "Accounting" },
            { "المخزون",    "Inventory" },
            { "المبيعات",   "Sales" },
            { "المشتريات",  "Purchases" },
            { "الخزينة",    "Treasury" },
            { "التقارير",   "Reporting" },
        };

        /// <summary>
        /// Refreshes navigation visibility based on Feature flags.
        /// Called on startup and after profile changes.
        /// Phase 3: Progressive Complexity Layer — UI-Level visibility only.
        /// </summary>
        public async Task RefreshNavigationAsync()
        {
            try
            {
                // Load all features once
                using var scope = _serviceProvider.CreateScope();
                var featureService = scope.ServiceProvider.GetRequiredService<IFeatureService>();
                var result = await featureService.GetAllAsync();
                if (result.IsFailure)
                {
                    // Feature check failed — ensure all items remain visible
                    EnsureAllNavigationVisible();
                    return;
                }

                var enabledSet = new System.Collections.Generic.HashSet<string>(
                    result.Data.Where(f => f.IsEnabled).Select(f => f.FeatureKey),
                    StringComparer.OrdinalIgnoreCase);

                // Walk NavigationItems and toggle visibility for feature-mapped sections
                string currentSection = null;
                bool currentSectionVisible = true;

                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    foreach (var item in NavigationItems)
                    {
                        if (item.ItemType == NavigationItemType.Header)
                        {
                            currentSection = item.Title;
                            if (_sectionFeatureMap.TryGetValue(currentSection, out var featureKey))
                            {
                                // Default to visible if feature not found in DB
                                currentSectionVisible = !enabledSet.Any() || enabledSet.Contains(featureKey);
                                item.IsVisible = currentSectionVisible;
                            }
                            else
                            {
                                currentSectionVisible = true;
                                item.IsVisible = true;
                            }
                        }
                        else if (item.ItemType == NavigationItemType.Separator)
                        {
                            item.IsVisible = currentSectionVisible;
                        }
                        else if (item.ItemType == NavigationItemType.Item)
                        {
                            if (currentSection != null && _sectionFeatureMap.ContainsKey(currentSection))
                            {
                                item.IsVisible = currentSectionVisible && IsVisibleForUser(item.PermissionKey);
                            }
                        }
                    }

                    // Fix separator visibility — separator should match its following header section
                    for (int i = 0; i < NavigationItems.Count; i++)
                    {
                        if (NavigationItems[i].ItemType == NavigationItemType.Separator)
                        {
                            bool nextSectionVisible = true;
                            for (int j = i + 1; j < NavigationItems.Count; j++)
                            {
                                if (NavigationItems[j].ItemType == NavigationItemType.Header)
                                {
                                    nextSectionVisible = NavigationItems[j].IsVisible;
                                    break;
                                }
                            }
                            NavigationItems[i].IsVisible = nextSectionVisible;
                        }
                    }
                });
            }
            catch
            {
                // Feature check is best-effort; ensure all navigation stays visible
                EnsureAllNavigationVisible();
            }
        }

        private void EnsureAllNavigationVisible()
        {
            try
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    foreach (var item in NavigationItems)
                    {
                        if (item.ItemType == NavigationItemType.Header || item.ItemType == NavigationItemType.Separator)
                        {
                            item.IsVisible = true;
                        }
                        else if (item.ItemType == NavigationItemType.Item)
                        {
                            item.IsVisible = IsVisibleForUser(item.PermissionKey);
                        }
                    }
                });
            }
            catch { /* UI thread safety */ }
        }
    }
}

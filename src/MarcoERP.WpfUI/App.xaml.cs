using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using MarcoERP.Persistence;
using MarcoERP.Persistence.Interceptors;
using MarcoERP.Persistence.Repositories;
using MarcoERP.Persistence.Seeds;
using MarcoERP.Persistence.Services;
using MarcoERP.Domain.Interfaces;
using MarcoERP.Domain.Entities.Accounting.Policies;
using MarcoERP.Application.Interfaces;
using MarcoERP.Application.Interfaces.Accounting;
using MarcoERP.Application.Services.Accounting;
using MarcoERP.Infrastructure.Services;
using MarcoERP.Infrastructure.Security;
using FluentValidation;
using MarcoERP.Application.DTOs.Accounting;
using MarcoERP.Application.DTOs.Inventory;
using MarcoERP.Application.Validators.Accounting;
using MarcoERP.Application.Validators.Inventory;
using MarcoERP.Application.Interfaces.Inventory;
using MarcoERP.Application.Services.Inventory;
using MarcoERP.Domain.Interfaces.Inventory;
using MarcoERP.Persistence.Repositories.Inventory;
using MarcoERP.Domain.Interfaces.Sales;
using MarcoERP.Domain.Interfaces.Purchases;
using MarcoERP.Persistence.Repositories.Sales;
using MarcoERP.Persistence.Repositories.Purchases;
using MarcoERP.Application.DTOs.Sales;
using MarcoERP.Application.DTOs.Purchases;
using MarcoERP.Application.Validators.Sales;
using MarcoERP.Application.Validators.Purchases;
using MarcoERP.Application.Interfaces.Sales;
using MarcoERP.Application.Interfaces.Purchases;
using MarcoERP.Application.Services.Sales;
using MarcoERP.Application.Services.Purchases;
using MarcoERP.Application.Services.Common;
using MarcoERP.Domain.Interfaces.Treasury;
using MarcoERP.Persistence.Repositories.Treasury;
using MarcoERP.Application.DTOs.Treasury;
using MarcoERP.Application.Validators.Treasury;
using MarcoERP.Application.Interfaces.Treasury;
using MarcoERP.Application.Services.Treasury;
using MarcoERP.Application.Interfaces.Reports;
using MarcoERP.Application.Interfaces.SmartEntry;
using MarcoERP.Application.Interfaces.Search;
using MarcoERP.Application.Interfaces.Security;
using MarcoERP.Application.Interfaces.Settings;
using MarcoERP.Application.Services.Security;
using MarcoERP.Application.Services.Settings;
using MarcoERP.Application.DTOs.Security;
using MarcoERP.Application.DTOs.Settings;
using MarcoERP.Application.Validators.Security;
using MarcoERP.Application.Validators.Settings;
using MarcoERP.Domain.Interfaces.Security;
using MarcoERP.Domain.Interfaces.Settings;
using MarcoERP.Persistence.Repositories.Security;
using MarcoERP.Persistence.Repositories.Settings;
using MarcoERP.Persistence.Services.Reports;
using MarcoERP.Application.Services.Reports;
using MarcoERP.Persistence.Services.SmartEntry;
using MarcoERP.Persistence.Services.Search;
using MarcoERP.Persistence.Services.Settings;
using MarcoERP.WpfUI.ViewModels.Sales;
using MarcoERP.WpfUI.Views.Sales;
using Microsoft.Extensions.Logging;
using MarcoERP.WpfUI.Navigation;
using MarcoERP.WpfUI.Services;
using MarcoERP.WpfUI.Views.Shell;
using MarcoERP.WpfUI.ViewModels;
using MarcoERP.WpfUI.Views;
using MarcoERP.WpfUI.ViewModels.Accounting;
using MarcoERP.WpfUI.ViewModels.Inventory;
using MarcoERP.WpfUI.ViewModels.Purchases;
using MarcoERP.WpfUI.ViewModels.Treasury;
using MarcoERP.WpfUI.ViewModels.Reports;
using MarcoERP.WpfUI.ViewModels.Settings;
using MarcoERP.WpfUI.ViewModels.Shell;
using MarcoERP.WpfUI.Views.Accounting;
using MarcoERP.WpfUI.Views.Inventory;
using MarcoERP.WpfUI.Views.Purchases;
using MarcoERP.WpfUI.Views.Treasury;
using MarcoERP.WpfUI.Views.Reports;
using MarcoERP.WpfUI.Views.Settings;
using MarcoERP.WpfUI.ViewModels.Common;
using MarcoERP.WpfUI.Views.Common;
using MarcoERP.Application.Reporting.Interfaces;
using MarcoERP.WpfUI.Reporting;

namespace MarcoERP.WpfUI
{
    /// <summary>
    /// Application entry point — Composition Root for Dependency Injection.
    /// </summary>
    public partial class App : System.Windows.Application
    {
        /// <summary>Phase 5: Code version for integrity checks.</summary>
        public static string CurrentAppVersion => "1.1.0";

        private IServiceProvider _serviceProvider;
        private IConfiguration _configuration;
        private IBackgroundJobService _backgroundJobService;

        // ── Phase 6: Migration Guard state ──
        private int _pendingMigrationCount;

        /// <summary>Global access to the DI container (WPF single-window pattern).</summary>
        public static IServiceProvider Services { get; private set; }

        /// <summary>
        /// Returns true when AppSettings:VatModel is "Inclusive".
        /// Governance: ACCOUNTING_PRINCIPLES VAT-03.
        /// </summary>
        public static bool IsVatInclusive
        {
            get
            {
                var config = Services?.GetService(typeof(IConfiguration)) as IConfiguration;
                var model = config?["AppSettings:VatModel"];
                return string.Equals(model, "Inclusive", StringComparison.OrdinalIgnoreCase);
            }
        }

        public App()
        {
            // Handle any unhandled exceptions
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private static void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(
                $"خطأ غير معالج:\n\n{e.Exception.Message}",
                "خطأ في التطبيق",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            e.Handled = true;
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            MessageBox.Show(
                $"خطأ حرج:\n\n{ex?.Message}",
                "خطأ حرج",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Global: Select all text in TextBox on focus (improves data entry for prices/amounts)
            EventManager.RegisterClassHandler(typeof(System.Windows.Controls.TextBox),
                System.Windows.Controls.TextBox.GotKeyboardFocusEvent,
                new KeyboardFocusChangedEventHandler((s, _) =>
                {
                    if (s is System.Windows.Controls.TextBox tb)
                        tb.SelectAll();
                }));
            EventManager.RegisterClassHandler(typeof(System.Windows.Controls.TextBox),
                System.Windows.Controls.TextBox.PreviewMouseLeftButtonDownEvent,
                new MouseButtonEventHandler((s, me) =>
                {
                    if (s is System.Windows.Controls.TextBox tb && !tb.IsKeyboardFocusWithin)
                    {
                        me.Handled = true;
                        tb.Focus();
                    }
                }));

            try
            {
                // Build configuration
                _configuration = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();

                // Build DI container
                var services = new ServiceCollection();
                ConfigureServices(services);
                _serviceProvider = services.BuildServiceProvider();
                Services = _serviceProvider;

                var databaseReady = await InitializeDatabaseAsync();

                if (databaseReady)
                {
                    // Start background jobs
                    _backgroundJobService = _serviceProvider.GetRequiredService<IBackgroundJobService>();
                    _backgroundJobService.StartAll();
                }

                var loginWindow = _serviceProvider.GetRequiredService<LoginWindow>();
                MainWindow = loginWindow;
                loginWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"فشل تهيئة التطبيق:\n\n{ex.Message}\n\n{ex.StackTrace}",
                    "خطأ في التهيئة",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Environment.Exit(1);
            }
        }

        private async Task<bool> InitializeDatabaseAsync()
        {
            var applyMigrations = _configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup");
            var seedData = _configuration.GetValue<bool>("Database:SeedOnStartup");

            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<MarcoDbContext>();

            try
            {
                var canConnect = await dbContext.Database.CanConnectAsync();
                if (!canConnect)
                {
                    MessageBox.Show(
                        "لا يمكن الاتصال بقاعدة البيانات.\nجاري إنشاء قاعدة البيانات...",
                        "معلومات الاتصال",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }

                if (applyMigrations)
                {
                    // ── Phase 6: Startup Migration Guard ─────────────
                    var pendingMigrations = (await dbContext.Database
                        .GetPendingMigrationsAsync()).ToList();

                    _pendingMigrationCount = pendingMigrations.Count;
                    // ── End Phase 6 Guard ────────────────────────────

                    // Apply migrations normally (controlled or direct)
                    await dbContext.Database.MigrateAsync();
                }
                else
                {
                    await dbContext.Database.EnsureCreatedAsync();
                }

                if (!seedData)
                    return true;

                await SystemAccountSeed.SeedAsync(dbContext);
                await CompanySeed.SeedAsync(dbContext);
                await UnitSeed.SeedAsync(dbContext);

                // Governance: CFG-01, DPR-03 — Never store passwords in source control.
                // Priority: Environment variable > appsettings (which should be empty in production).
                var adminSeedPassword = Environment.GetEnvironmentVariable("MARCOERP_ADMIN_PASSWORD")
                    ?? _configuration["Security:AdminSeedPassword"];

                if (string.IsNullOrWhiteSpace(adminSeedPassword))
                    throw new InvalidOperationException("Admin seed password is required when SeedOnStartup is enabled.");

                var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
                await SecuritySeed.SeedAsync(dbContext, passwordHasher.HashPassword(adminSeedPassword));
                await SystemSettingSeed.SeedAsync(dbContext);
                await FeatureSeed.SeedAsync(dbContext);
                await ProfileSeed.SeedAsync(dbContext);
                await VersionSeed.SeedAsync(dbContext);

                return true;
            }
            catch (Exception dbEx)
            {
                var errorDetails = $@"❌ فشل الاتصال بقاعدة البيانات

🔴 تفاصيل الخطأ:
━━━━━━━━━━━━━━━━━━━━━━━━
{dbEx.Message}

💡 تأكد من:
1. تشغيل SQL Server (.\\SQL2022)
2. صلاحيات Windows Authentication
3. اسم الـ Instance صحيح";

                MessageBox.Show(
                    errorDetails,
                    "خطأ في الاتصال بقاعدة البيانات",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _backgroundJobService?.StopAll();
            _backgroundJobService?.Dispose();
            (_serviceProvider as IDisposable)?.Dispose();
            base.OnExit(e);
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Configuration
            services.AddSingleton(_configuration);

            // ─── Persistence Layer ───
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            services.AddScoped<AuditSaveChangesInterceptor>();
            services.AddSingleton<HardDeleteProtectionInterceptor>();

            services.AddDbContext<MarcoDbContext>((serviceProvider, options) =>
            {
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(typeof(MarcoDbContext).Assembly.FullName);
                    // Note: EnableRetryOnFailure removed - incompatible with user-initiated transactions
                });

                // Register interceptors
                var auditInterceptor = serviceProvider.GetRequiredService<AuditSaveChangesInterceptor>();
                var hardDeleteGuard = serviceProvider.GetRequiredService<HardDeleteProtectionInterceptor>();
                options.AddInterceptors(auditInterceptor, hardDeleteGuard);
            });

            // ─── Domain Repositories ───
            services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddScoped<IJournalEntryRepository, JournalEntryRepository>();
            services.AddScoped<IFiscalYearRepository, FiscalYearRepository>();
            services.AddScoped<IAuditLogRepository, AuditLogRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // ─── Inventory Repositories ───
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IUnitRepository, UnitRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IWarehouseRepository, WarehouseRepository>();
            services.AddScoped<IWarehouseProductRepository, WarehouseProductRepository>();
            services.AddScoped<IInventoryMovementRepository, InventoryMovementRepository>();

            // ─── Sales & Purchases Repositories ───
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<ISupplierRepository, SupplierRepository>();
            services.AddScoped<ISalesRepresentativeRepository, SalesRepresentativeRepository>();
            services.AddScoped<IPurchaseInvoiceRepository, PurchaseInvoiceRepository>();
            services.AddScoped<IPurchaseReturnRepository, PurchaseReturnRepository>();
            services.AddScoped<ISalesInvoiceRepository, SalesInvoiceRepository>();
            services.AddScoped<ISalesReturnRepository, SalesReturnRepository>();
            services.AddScoped<IPosSessionRepository, PosSessionRepository>();
            services.AddScoped<IPosPaymentRepository, PosPaymentRepository>();
            services.AddScoped<IPriceListRepository, PriceListRepository>();
            services.AddScoped<IInventoryAdjustmentRepository, InventoryAdjustmentRepository>();
            services.AddScoped<ISalesQuotationRepository, SalesQuotationRepository>();
            services.AddScoped<IPurchaseQuotationRepository, PurchaseQuotationRepository>();

            // ─── Treasury Repositories ───
            services.AddScoped<ICashboxRepository, CashboxRepository>();
            services.AddScoped<IBankAccountRepository, BankAccountRepository>();
            services.AddScoped<IBankReconciliationRepository, BankReconciliationRepository>();
            services.AddScoped<ICashReceiptRepository, CashReceiptRepository>();
            services.AddScoped<ICashPaymentRepository, CashPaymentRepository>();
            services.AddScoped<ICashTransferRepository, CashTransferRepository>();

            // ─── Security & Settings Repositories ───
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<ISystemSettingRepository, SystemSettingRepository>();
            services.AddScoped<IFeatureRepository, FeatureRepository>();
            services.AddScoped<IProfileRepository, ProfileRepository>();

            // ─── Infrastructure Layer ───
            services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
            services.AddSingleton<IAlertService, AlertService>();
            services.AddSingleton<IActivityTracker, ActivityTracker>();
            services.AddSingleton<IBackgroundJobService, BackgroundJobService>();
            services.AddLogging(builder => builder.AddDebug());
            services.AddSingleton<ICurrentUserService, CurrentUserService>();
            services.AddSingleton<ICompanyContext, DefaultCompanyContext>();
            services.AddScoped<IAuditLogger, AuditLogger>();
            services.AddScoped<IJournalNumberGenerator, JournalNumberGenerator>();
            services.AddScoped<ICodeGenerator, CodeGenerator>();
            services.AddSingleton<IPasswordHasher, PasswordHasher>();

            // ─── FluentValidation Validators ───
            services.AddScoped<IValidator<CreateAccountDto>, CreateAccountDtoValidator>();
            services.AddScoped<IValidator<UpdateAccountDto>, UpdateAccountDtoValidator>();
            services.AddScoped<IValidator<CreateFiscalYearDto>, CreateFiscalYearDtoValidator>();
            services.AddScoped<IValidator<CreateJournalEntryDto>, CreateJournalEntryDtoValidator>();
            services.AddScoped<IValidator<ReverseJournalEntryDto>, ReverseJournalEntryDtoValidator>();

            // ─── Inventory Validators ───
            services.AddScoped<IValidator<CreateCategoryDto>, CreateCategoryDtoValidator>();
            services.AddScoped<IValidator<UpdateCategoryDto>, UpdateCategoryDtoValidator>();
            services.AddScoped<IValidator<CreateUnitDto>, CreateUnitDtoValidator>();
            services.AddScoped<IValidator<UpdateUnitDto>, UpdateUnitDtoValidator>();
            services.AddScoped<IValidator<CreateProductDto>, CreateProductDtoValidator>();
            services.AddScoped<IValidator<UpdateProductDto>, UpdateProductDtoValidator>();
            services.AddScoped<IValidator<CreateWarehouseDto>, CreateWarehouseDtoValidator>();
            services.AddScoped<IValidator<UpdateWarehouseDto>, UpdateWarehouseDtoValidator>();

            // ─── Sales & Purchases Validators ───
            services.AddScoped<IValidator<CreateCustomerDto>, CreateCustomerDtoValidator>();
            services.AddScoped<IValidator<UpdateCustomerDto>, UpdateCustomerDtoValidator>();
            services.AddScoped<IValidator<CreateSupplierDto>, CreateSupplierDtoValidator>();
            services.AddScoped<IValidator<UpdateSupplierDto>, UpdateSupplierDtoValidator>();
            services.AddScoped<IValidator<CreatePurchaseInvoiceDto>, CreatePurchaseInvoiceDtoValidator>();
            services.AddScoped<IValidator<UpdatePurchaseInvoiceDto>, UpdatePurchaseInvoiceDtoValidator>();
            services.AddScoped<IValidator<CreatePurchaseReturnDto>, CreatePurchaseReturnDtoValidator>();
            services.AddScoped<IValidator<UpdatePurchaseReturnDto>, UpdatePurchaseReturnDtoValidator>();
            services.AddScoped<IValidator<CreateSalesInvoiceDto>, CreateSalesInvoiceDtoValidator>();
            services.AddScoped<IValidator<UpdateSalesInvoiceDto>, UpdateSalesInvoiceDtoValidator>();
            services.AddScoped<IValidator<CreateSalesReturnDto>, CreateSalesReturnDtoValidator>();
            services.AddScoped<IValidator<UpdateSalesReturnDto>, UpdateSalesReturnDtoValidator>();
            services.AddScoped<IValidator<CreateSalesRepresentativeDto>, CreateSalesRepresentativeDtoValidator>();
            services.AddScoped<IValidator<UpdateSalesRepresentativeDto>, UpdateSalesRepresentativeDtoValidator>();
            services.AddScoped<IValidator<CreateSalesQuotationDto>, CreateSalesQuotationDtoValidator>();
            services.AddScoped<IValidator<UpdateSalesQuotationDto>, UpdateSalesQuotationDtoValidator>();
            services.AddScoped<IValidator<CreatePurchaseQuotationDto>, CreatePurchaseQuotationDtoValidator>();
            services.AddScoped<IValidator<UpdatePurchaseQuotationDto>, UpdatePurchaseQuotationDtoValidator>();

            // ─── POS Validators ───
            services.AddScoped<IValidator<OpenPosSessionDto>, OpenPosSessionDtoValidator>();
            services.AddScoped<IValidator<ClosePosSessionDto>, ClosePosSessionDtoValidator>();
            services.AddScoped<IValidator<CompletePoseSaleDto>, CompletePosSaleDtoValidator>();

            // ─── Treasury Validators ───
            services.AddScoped<IValidator<CreateCashboxDto>, CreateCashboxDtoValidator>();
            services.AddScoped<IValidator<UpdateCashboxDto>, UpdateCashboxDtoValidator>();
            services.AddScoped<IValidator<CreateBankAccountDto>, CreateBankAccountDtoValidator>();
            services.AddScoped<IValidator<UpdateBankAccountDto>, UpdateBankAccountDtoValidator>();
            services.AddScoped<IValidator<CreateBankReconciliationDto>, CreateBankReconciliationDtoValidator>();
            services.AddScoped<IValidator<CreateBankReconciliationItemDto>, CreateBankReconciliationItemDtoValidator>();
            services.AddScoped<IValidator<CreateCashReceiptDto>, CreateCashReceiptDtoValidator>();
            services.AddScoped<IValidator<UpdateCashReceiptDto>, UpdateCashReceiptDtoValidator>();
            services.AddScoped<IValidator<CreateCashPaymentDto>, CreateCashPaymentDtoValidator>();
            services.AddScoped<IValidator<UpdateCashPaymentDto>, UpdateCashPaymentDtoValidator>();
            services.AddScoped<IValidator<CreateCashTransferDto>, CreateCashTransferDtoValidator>();
            services.AddScoped<IValidator<UpdateCashTransferDto>, UpdateCashTransferDtoValidator>();

            // ─── Security & Settings Validators ───
            services.AddScoped<IValidator<CreateRoleDto>, CreateRoleDtoValidator>();
            services.AddScoped<IValidator<UpdateRoleDto>, UpdateRoleDtoValidator>();
            services.AddScoped<IValidator<CreateUserDto>, CreateUserDtoValidator>();
            services.AddScoped<IValidator<UpdateUserDto>, UpdateUserDtoValidator>();
            services.AddScoped<IValidator<ResetPasswordDto>, ResetPasswordDtoValidator>();
            services.AddScoped<IValidator<ChangePasswordDto>, ChangePasswordDtoValidator>();
            services.AddScoped<IValidator<LoginDto>, LoginDtoValidator>();
            services.AddScoped<IValidator<UpdateSystemSettingDto>, UpdateSystemSettingDtoValidator>();

            // ─── Application Layer ───
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IJournalEntryService, JournalEntryService>();
            services.AddScoped<IYearEndClosingService, YearEndClosingService>();
            services.AddScoped<IFiscalYearService, FiscalYearService>();

            // ─── Inventory Services ───
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IUnitService, UnitService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IWarehouseService, WarehouseService>();
            services.AddScoped<IBulkPriceUpdateService, BulkPriceUpdateService>();

            // ─── Sales & Purchases Services ───
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<ISupplierService, SupplierService>();
            services.AddScoped<ISalesRepresentativeService, SalesRepresentativeService>();
            services.AddScoped<IPurchaseInvoiceService, PurchaseInvoiceService>();
            services.AddScoped<PurchaseInvoiceRepositories>();
            services.AddScoped<PurchaseInvoiceServices>();
            services.AddScoped<PurchaseInvoiceValidators>();
            services.AddScoped<IPurchaseReturnService, PurchaseReturnService>();
            services.AddScoped<PurchaseReturnRepositories>();
            services.AddScoped<PurchaseReturnServices>();
            services.AddScoped<PurchaseReturnValidators>();
            services.AddScoped<ISalesInvoiceService, SalesInvoiceService>();
            services.AddScoped<SalesInvoiceRepositories>();
            services.AddScoped<SalesInvoiceServices>();
            services.AddScoped<SalesInvoiceValidators>();
            services.AddScoped<ISalesReturnService, SalesReturnService>();
            services.AddScoped<SalesReturnRepositories>();
            services.AddScoped<SalesReturnServices>();
            services.AddScoped<SalesReturnValidators>();
            services.AddScoped<IPosService, PosService>();
            services.AddScoped<PosSalesRepositories>();
            services.AddScoped<PosInventoryRepositories>();
            services.AddScoped<PosAccountingRepositories>();
            services.AddScoped<PosRepositories>();
            services.AddScoped<PosServices>();
            services.AddScoped<PosValidators>();
            services.AddScoped<IPriceListService, PriceListService>();
            services.AddScoped<IInventoryAdjustmentService, InventoryAdjustmentService>();
            services.AddScoped<ISalesQuotationService, SalesQuotationService>();
            services.AddScoped<IPurchaseQuotationService, PurchaseQuotationService>();

            // ─── Treasury Services ───
            services.AddScoped<ICashboxService, CashboxService>();
            services.AddScoped<IBankAccountService, BankAccountService>();
            services.AddScoped<IBankReconciliationService, BankReconciliationService>();
            services.AddScoped<ICashReceiptService, CashReceiptService>();
            services.AddScoped<CashReceiptRepositories>();
            services.AddScoped<CashReceiptServices>();
            services.AddScoped<CashReceiptValidators>();
            services.AddScoped<ICashPaymentService, CashPaymentService>();
            services.AddScoped<CashPaymentRepositories>();
            services.AddScoped<CashPaymentServices>();
            services.AddScoped<CashPaymentValidators>();
            services.AddScoped<ICashTransferService, CashTransferService>();
            services.AddScoped<CashTransferRepositories>();
            services.AddScoped<CashTransferServices>();
            services.AddScoped<CashTransferValidators>();

            services.AddScoped<ITreasuryInvoicePaymentQueryService, TreasuryInvoicePaymentQueryService>();

            // ─── Security & Settings Services ───
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<ISystemSettingsService, SystemSettingsService>();
            services.AddScoped<IFeatureService, FeatureService>();
            services.AddScoped<IProfileService, ProfileService>();
            services.AddScoped<IImpactAnalyzerService, ImpactAnalyzerService>();
            services.AddScoped<IVersionRepository, VersionRepository>();
            services.AddScoped<IVersionService, VersionService>();
            // Phase 8D: Module Dependency Inspector (reflection-based, report-only)
            services.AddSingleton<IModuleDependencyInspector>(sp =>
                new MarcoERP.Persistence.Services.Settings.ModuleDependencyInspector(
                    typeof(MarcoERP.Application.Common.ModuleAttribute).Assembly,
                    typeof(MarcoERP.Persistence.MarcoDbContext).Assembly));
            services.AddScoped<IIntegrityCheckService>(sp =>
                new MarcoERP.Persistence.Services.Settings.GovernanceIntegrityCheckService(
                    sp.GetRequiredService<MarcoDbContext>(),
                    () => CurrentAppVersion,
                    sp.GetRequiredService<IModuleDependencyInspector>()));
            services.AddScoped<IBackupService, BackupService>();
            services.AddScoped<IDatabaseBackupService, DatabaseBackupService>();
            services.AddScoped<IMigrationExecutionService, MigrationExecutionService>();
            services.AddScoped<IGovernanceAuditService, GovernanceAuditService>();
            services.AddScoped<IAuditLogService, AuditLogService>();
            services.AddScoped<IIntegrityService, IntegrityService>();

            // ─── Reports Service ───
            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<IReportExportService, ReportExportService>();

            // ─── Interactive Reporting Framework ───
            services.AddSingleton<IDrillDownResolver, DrillDownResolver>();
            services.AddSingleton<DrillDownEngine>();

            // ─── Product Import ───
            services.AddScoped<IProductImportService, MarcoERP.Application.Services.Inventory.ProductImportService>();

            // ─── Common Calculations ───
            services.AddSingleton<ILineCalculationService, LineCalculationService>();

            // ─── Smart Entry (read-only queries) ───
            services.AddScoped<ISmartEntryQueryService, SmartEntryQueryService>();

            // ─── Global Search (Ctrl+K) ───
            services.AddScoped<IGlobalSearchQueryService, GlobalSearchQueryService>();

            // ─── Navigation & Window Services ───
            services.AddSingleton<IWindowService, WindowService>();
            services.AddSingleton<IQuickTreasuryDialogService, QuickTreasuryDialogService>();
            services.AddScoped<IInvoiceTreasuryIntegrationService, InvoiceTreasuryIntegrationService>();
            services.AddSingleton<IInvoicePdfPreviewService, InvoicePdfPreviewService>();
            services.AddSingleton<IViewRegistry>(sp =>
            {
                var registry = new ViewRegistry();

                registry.Register<DashboardView, DashboardViewModel>("Dashboard", "لوحة التحكم");
                registry.Register<ChartOfAccountsView, ChartOfAccountsViewModel>("ChartOfAccounts", "شجرة الحسابات");
                registry.Register<JournalEntryView, JournalEntryViewModel>("JournalEntries", "القيود اليومية");
                registry.Register<FiscalPeriodView, FiscalPeriodViewModel>("FiscalPeriods", "الفترات المالية");
                registry.Register<OpeningBalanceWizardView, OpeningBalanceWizardViewModel>("OpeningBalance", "الأرصدة الافتتاحية");

                registry.Register<CategoryView, CategoryViewModel>("Categories", "التصنيفات");
                registry.Register<UnitView, UnitViewModel>("Units", "وحدات القياس");
                registry.Register<ProductView, ProductViewModel>("Products", "الأصناف");
                registry.Register<WarehouseView, WarehouseViewModel>("Warehouses", "المخازن");
                registry.Register<BulkPriceUpdateView, BulkPriceUpdateViewModel>("BulkPriceUpdate", "تحديث الأسعار الجماعي");
                registry.Register<InventoryAdjustmentListView, InventoryAdjustmentListViewModel>("InventoryAdjustments", "تسويات المخزون");
                registry.Register<InventoryAdjustmentDetailView, InventoryAdjustmentDetailViewModel>("InventoryAdjustmentDetail", "تسوية مخزون");
                registry.Register<ProductImportView, ProductImportViewModel>("ProductImport", "استيراد الأصناف");

                registry.Register<SalesInvoiceListView, SalesInvoiceListViewModel>("SalesInvoices", "فواتير البيع");
                registry.Register<SalesInvoiceDetailView, SalesInvoiceDetailViewModel>("SalesInvoiceDetail", "فاتورة بيع");
                registry.Register<SalesReturnListView, SalesReturnListViewModel>("SalesReturns", "مرتجعات البيع");
                registry.Register<SalesReturnDetailView, SalesReturnDetailViewModel>("SalesReturnDetail", "مرتجع بيع");
                registry.Register<CustomerView, CustomerViewModel>("Customers", "العملاء");
                registry.Register<SalesRepresentativeView, SalesRepresentativeViewModel>("SalesRepresentatives", "مندوبي المبيعات");
                registry.Register<PriceListView, PriceListViewModel>("PriceLists", "قوائم الأسعار");
                registry.Register<SalesQuotationListView, SalesQuotationListViewModel>("SalesQuotations", "عروض أسعار البيع");
                registry.Register<SalesQuotationDetailView, SalesQuotationDetailViewModel>("SalesQuotationDetail", "عرض سعر بيع");

                registry.Register<PurchaseInvoiceListView, PurchaseInvoiceListViewModel>("PurchaseInvoices", "فواتير الشراء");
                registry.Register<PurchaseInvoiceDetailView, PurchaseInvoiceDetailViewModel>("PurchaseInvoiceDetail", "فاتورة شراء");
                registry.Register<PurchaseReturnListView, PurchaseReturnListViewModel>("PurchaseReturns", "مرتجعات الشراء");
                registry.Register<PurchaseReturnDetailView, PurchaseReturnDetailViewModel>("PurchaseReturnDetail", "مرتجع شراء");
                registry.Register<SupplierView, SupplierViewModel>("Suppliers", "الموردين");
                registry.Register<PurchaseQuotationListView, PurchaseQuotationListViewModel>("PurchaseQuotations", "طلبات الشراء");
                registry.Register<PurchaseQuotationDetailView, PurchaseQuotationDetailViewModel>("PurchaseQuotationDetail", "طلب شراء");

                registry.Register<CashboxView, CashboxViewModel>("Cashboxes", "الخزن");
                registry.Register<BankAccountView, BankAccountViewModel>("BankAccounts", "الحسابات البنكية");
                registry.Register<BankReconciliationView, BankReconciliationViewModel>("BankReconciliation", "التسوية البنكية");
                registry.Register<CashReceiptView, CashReceiptViewModel>("CashReceipts", "سندات القبض");
                registry.Register<CashPaymentView, CashPaymentViewModel>("CashPayments", "سندات الصرف");
                registry.Register<CashTransferView, CashTransferViewModel>("CashTransfers", "التحويلات");

                registry.Register<ReportHubView, ReportHubViewModel>("Reports", "التقارير");
                registry.Register<TrialBalanceView, TrialBalanceViewModel>("TrialBalance", "ميزان المراجعة");
                registry.Register<AccountStatementView, AccountStatementViewModel>("AccountStatement", "كشف حساب");
                registry.Register<IncomeStatementView, IncomeStatementViewModel>("IncomeStatement", "قائمة الدخل");
                registry.Register<BalanceSheetView, BalanceSheetViewModel>("BalanceSheet", "الميزانية العمومية");
                registry.Register<SalesReportView, SalesReportViewModel>("SalesReport", "تقرير المبيعات");
                registry.Register<PurchaseReportView, PurchaseReportViewModel>("PurchaseReport", "تقرير المشتريات");
                registry.Register<ProfitReportView, ProfitReportViewModel>("ProfitReport", "تقرير الأرباح");
                registry.Register<VatReportView, VatReportViewModel>("VatReport", "تقرير الضريبة");
                registry.Register<InventoryReportView, InventoryReportViewModel>("InventoryReport", "تقرير المخزون");
                registry.Register<StockCardView, StockCardViewModel>("StockCard", "بطاقة الصنف");
                registry.Register<CashboxMovementView, CashboxMovementViewModel>("CashboxMovement", "حركة الخزنة");
                registry.Register<AgingReportView, AgingReportViewModel>("AgingReport", "أعمار الديون");

                registry.Register<FiscalYearView, FiscalYearViewModel>("FiscalYear", "السنة المالية");
                registry.Register<SystemSettingsView, SystemSettingsViewModel>("SystemSettings", "إعدادات النظام");
                registry.Register<UserManagementView, UserManagementViewModel>("UserManagement", "إدارة المستخدمين");
                registry.Register<RoleManagementView, RoleManagementViewModel>("RoleManagement", "إدارة الأدوار");
                registry.Register<AuditLogView, AuditLogViewModel>("AuditLog", "سجل المراجعة");
                registry.Register<BackupSettingsView, BackupSettingsViewModel>("BackupSettings", "النسخ الاحتياطي");
                registry.Register<IntegrityCheckView, IntegrityCheckViewModel>("IntegrityCheck", "فحص السلامة");
                registry.Register<GovernanceConsoleView, GovernanceConsoleViewModel>("GovernanceConsole", "وحدة التحكم");
                registry.Register<GovernanceIntegrityView, GovernanceIntegrityViewModel>("GovernanceIntegrity", "فحص سلامة الحوكمة");
                registry.Register<MigrationCenterView, MigrationCenterViewModel>("MigrationCenter", "مركز التحديثات");

                return registry;
            });
            services.AddSingleton<INavigationService, NavigationService>();

            // ─── WPF Views & ViewModels ───
            services.AddSingleton<MainWindowViewModel>();
            services.AddTransient<MainWindow>();
            services.AddTransient<LoginViewModel>();
            services.AddTransient<LoginWindow>();

            services.AddTransient<DashboardView>();
            services.AddTransient<DashboardViewModel>();

            services.AddTransient<ChartOfAccountsView>();
            services.AddTransient<ChartOfAccountsViewModel>();
            services.AddTransient<JournalEntryView>();
            services.AddTransient<JournalEntryViewModel>();
            services.AddTransient<FiscalPeriodView>();
            services.AddTransient<FiscalPeriodViewModel>();
            services.AddTransient<OpeningBalanceWizardView>();
            services.AddTransient<OpeningBalanceWizardViewModel>();

            services.AddTransient<CategoryView>();
            services.AddTransient<CategoryViewModel>();
            services.AddTransient<UnitView>();
            services.AddTransient<UnitViewModel>();
            services.AddTransient<ProductView>();
            services.AddTransient<ProductViewModel>();
            services.AddTransient<WarehouseView>();
            services.AddTransient<WarehouseViewModel>();
            services.AddTransient<BulkPriceUpdateView>();
            services.AddTransient<BulkPriceUpdateViewModel>();
            services.AddTransient<InventoryAdjustmentListView>();
            services.AddTransient<InventoryAdjustmentListViewModel>();
            services.AddTransient<InventoryAdjustmentDetailView>();
            services.AddTransient<InventoryAdjustmentDetailViewModel>();

            services.AddTransient<SalesInvoiceView>();
            services.AddTransient<SalesInvoiceViewModel>();
            services.AddTransient<SalesInvoiceListView>();
            services.AddTransient<SalesInvoiceListViewModel>();
            services.AddTransient<SalesInvoiceDetailView>();
            services.AddTransient<SalesInvoiceDetailViewModel>();
            services.AddTransient<SalesReturnView>();
            services.AddTransient<SalesReturnViewModel>();
            services.AddTransient<SalesReturnListView>();
            services.AddTransient<SalesReturnListViewModel>();
            services.AddTransient<SalesReturnDetailView>();
            services.AddTransient<SalesReturnDetailViewModel>();
            services.AddTransient<CustomerView>();
            services.AddTransient<CustomerViewModel>();
            services.AddTransient<SalesRepresentativeView>();
            services.AddTransient<SalesRepresentativeViewModel>();
            services.AddTransient<PriceListView>();
            services.AddTransient<PriceListViewModel>();
            services.AddTransient<PosWindow>();
            services.AddTransient<PosViewModel>();
            services.AddTransient<SalesQuotationListView>();
            services.AddTransient<SalesQuotationListViewModel>();
            services.AddTransient<SalesQuotationDetailView>();
            services.AddTransient<SalesQuotationDetailViewModel>();

            services.AddTransient<PurchaseInvoiceView>();
            services.AddTransient<PurchaseInvoiceViewModel>();
            services.AddTransient<PurchaseInvoiceListView>();
            services.AddTransient<PurchaseInvoiceListViewModel>();
            services.AddTransient<PurchaseInvoiceDetailView>();
            services.AddTransient<PurchaseInvoiceDetailViewModel>();
            services.AddTransient<PurchaseReturnView>();
            services.AddTransient<PurchaseReturnViewModel>();
            services.AddTransient<PurchaseReturnListView>();
            services.AddTransient<PurchaseReturnListViewModel>();
            services.AddTransient<PurchaseReturnDetailView>();
            services.AddTransient<PurchaseReturnDetailViewModel>();
            services.AddTransient<SupplierView>();
            services.AddTransient<SupplierViewModel>();
            services.AddTransient<PurchaseQuotationListView>();
            services.AddTransient<PurchaseQuotationListViewModel>();
            services.AddTransient<PurchaseQuotationDetailView>();
            services.AddTransient<PurchaseQuotationDetailViewModel>();

            services.AddTransient<CashboxView>();
            services.AddTransient<CashboxViewModel>();
            services.AddTransient<BankAccountView>();
            services.AddTransient<BankAccountViewModel>();
            services.AddTransient<BankReconciliationView>();
            services.AddTransient<BankReconciliationViewModel>();
            services.AddTransient<CashReceiptView>();
            services.AddTransient<CashReceiptViewModel>();
            services.AddTransient<CashPaymentView>();
            services.AddTransient<CashPaymentViewModel>();
            services.AddTransient<CashTransferView>();
            services.AddTransient<CashTransferViewModel>();

            services.AddTransient<ReportHubView>();
            services.AddTransient<ReportHubViewModel>();
            services.AddTransient<TrialBalanceView>();
            services.AddTransient<TrialBalanceViewModel>();
            services.AddTransient<AccountStatementView>();
            services.AddTransient<AccountStatementViewModel>();
            services.AddTransient<IncomeStatementView>();
            services.AddTransient<IncomeStatementViewModel>();
            services.AddTransient<BalanceSheetView>();
            services.AddTransient<BalanceSheetViewModel>();
            services.AddTransient<SalesReportView>();
            services.AddTransient<SalesReportViewModel>();
            services.AddTransient<PurchaseReportView>();
            services.AddTransient<PurchaseReportViewModel>();
            services.AddTransient<ProfitReportView>();
            services.AddTransient<ProfitReportViewModel>();
            services.AddTransient<VatReportView>();
            services.AddTransient<VatReportViewModel>();
            services.AddTransient<InventoryReportView>();
            services.AddTransient<InventoryReportViewModel>();
            services.AddTransient<StockCardView>();
            services.AddTransient<StockCardViewModel>();
            services.AddTransient<CashboxMovementView>();
            services.AddTransient<CashboxMovementViewModel>();
            services.AddTransient<AgingReportView>();
            services.AddTransient<AgingReportViewModel>();

            services.AddTransient<FiscalYearView>();
            services.AddTransient<FiscalYearViewModel>();
            services.AddTransient<SystemSettingsView>();
            services.AddTransient<SystemSettingsViewModel>();
            services.AddTransient<GovernanceConsoleView>();
            services.AddTransient<GovernanceConsoleViewModel>();
            services.AddTransient<GovernanceIntegrityView>();
            services.AddTransient<GovernanceIntegrityViewModel>();
            services.AddTransient<MigrationCenterView>();
            services.AddTransient<MigrationCenterViewModel>();
            services.AddTransient<UserManagementView>();
            services.AddTransient<UserManagementViewModel>();
            services.AddTransient<RoleManagementView>();
            services.AddTransient<RoleManagementViewModel>();
            services.AddTransient<AuditLogView>();
            services.AddTransient<AuditLogViewModel>();
            services.AddTransient<BackupSettingsView>();
            services.AddTransient<BackupSettingsViewModel>();
            services.AddTransient<IntegrityCheckView>();
            services.AddTransient<IntegrityCheckViewModel>();

            services.AddTransient<QuickCashReceiptViewModel>();
            services.AddTransient<QuickCashReceiptWindow>();
            services.AddTransient<QuickCashPaymentViewModel>();
            services.AddTransient<QuickCashPaymentWindow>();

            // ─── Common Dialogs ───
            services.AddTransient<QuickTreasuryDialogViewModel>();
            services.AddTransient<QuickTreasuryDialog>();
            services.AddTransient<InvoicePdfPreviewDialogViewModel>();
            services.AddTransient<InvoicePdfPreviewDialog>();
        }

        /// <summary>
        /// Resolves a service from the DI container.
        /// </summary>
        public T GetService<T>() where T : class
        {
            return _serviceProvider.GetService<T>();
        }

        /// <summary>
        /// Resolves a required service from the DI container.
        /// </summary>
        public T GetRequiredService<T>() where T : class
        {
            return _serviceProvider.GetRequiredService<T>();
        }
    }
}
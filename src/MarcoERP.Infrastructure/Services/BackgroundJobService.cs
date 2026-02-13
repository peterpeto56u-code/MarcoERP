using System;
using System.IO;
using System.Timers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MarcoERP.Application.Interfaces;
using MarcoERP.Application.Interfaces.Settings;
using MarcoERP.Domain.Interfaces.Inventory;

namespace MarcoERP.Infrastructure.Services
{
    /// <summary>
    /// Lightweight background job runner for WPF desktop.
    /// Uses System.Timers.Timer for periodic execution — no ASP.NET hosting required.
    /// Resolves scoped services via IServiceProvider for each tick.
    /// </summary>
    public sealed class BackgroundJobService : IBackgroundJobService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BackgroundJobService> _logger;
        private readonly IAlertService _alertService;
        private readonly IActivityTracker _activityTracker;
        private readonly IDateTimeProvider _dateTime;

        private Timer _backupTimer;
        private Timer _sessionWatchdogTimer;
        private Timer _lowStockTimer;

        private bool _disposed;

        // ─── Default intervals ───
        private static readonly TimeSpan BackupInterval = TimeSpan.FromHours(24);
        private static readonly TimeSpan SessionWatchdogInterval = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan LowStockInterval = TimeSpan.FromMinutes(30);
        private static readonly TimeSpan IdleTimeout = TimeSpan.FromMinutes(30);

        public BackgroundJobService(
            IServiceProvider serviceProvider,
            ILogger<BackgroundJobService> logger,
            IAlertService alertService,
            IActivityTracker activityTracker,
            IDateTimeProvider dateTime)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _alertService = alertService ?? throw new ArgumentNullException(nameof(alertService));
            _activityTracker = activityTracker ?? throw new ArgumentNullException(nameof(activityTracker));
            _dateTime = dateTime ?? throw new ArgumentNullException(nameof(dateTime));
        }

        /// <inheritdoc />
        public void StartAll()
        {
            _logger.LogInformation("Background jobs starting…");

            _backupTimer = CreateTimer(BackupInterval, OnAutoBackupTick);
            _sessionWatchdogTimer = CreateTimer(SessionWatchdogInterval, OnSessionWatchdogTick);
            _lowStockTimer = CreateTimer(LowStockInterval, OnLowStockTick);

            // Fire low-stock check once at startup (after a short delay).
            _lowStockTimer.Interval = TimeSpan.FromSeconds(30).TotalMilliseconds;
            _lowStockTimer.Start();

            _backupTimer.Start();
            _sessionWatchdogTimer.Start();

            _logger.LogInformation("Background jobs started.");
        }

        /// <inheritdoc />
        public void StopAll()
        {
            _logger.LogInformation("Background jobs stopping…");

            StopTimer(ref _backupTimer);
            StopTimer(ref _sessionWatchdogTimer);
            StopTimer(ref _lowStockTimer);

            _logger.LogInformation("Background jobs stopped.");
        }

        // ────────────────────────── Job 1: Auto-Backup ──────────────────────────

        private async void OnAutoBackupTick(object sender, ElapsedEventArgs e)
        {
            try
            {
                var backupDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "MarcoERP", "Backups");

                Directory.CreateDirectory(backupDir);

                var backupPath = Path.Combine(backupDir,
                    $"Auto_{_dateTime.UtcNow:yyyyMMdd_HHmmss}.bak");

                using var scope = _serviceProvider.CreateScope();
                var backupService = scope.ServiceProvider.GetRequiredService<IBackupService>();
                var result = await backupService.BackupAsync(backupPath);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("Auto-backup completed: {Path}", backupPath);
                }
                else
                {
                    _logger.LogWarning("Auto-backup failed: {Error}", result.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Auto-backup job encountered an unhandled error.");
            }
        }

        // ────────────────────────── Job 2: Session Watchdog ──────────────────────────

        private void OnSessionWatchdogTick(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (_activityTracker.IsIdle(IdleTimeout))
                {
                    _logger.LogInformation(
                        "User appears idle (no activity since {LastActivity:HH:mm:ss UTC}).",
                        _activityTracker.LastActivityUtc);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Session watchdog job encountered an unhandled error.");
            }
        }

        // ────────────────────────── Job 3: Low Stock Alert ──────────────────────────

        private async void OnLowStockTick(object sender, ElapsedEventArgs e)
        {
            try
            {
                // Reset interval to the standard period after the initial quick-fire.
                if (sender is Timer timer && timer.Interval < LowStockInterval.TotalMilliseconds)
                {
                    timer.Interval = LowStockInterval.TotalMilliseconds;
                }

                using var scope = _serviceProvider.CreateScope();
                var warehouseProductRepo = scope.ServiceProvider
                    .GetRequiredService<IWarehouseProductRepository>();

                var lowStockItems = await warehouseProductRepo.GetBelowMinimumStockAsync();

                // Clear previous low-stock alerts, then re-add current ones.
                _alertService.ClearAlerts("LowStock");

                if (lowStockItems == null || lowStockItems.Count == 0)
                {
                    _logger.LogDebug("Low-stock check: all products above minimum.");
                    return;
                }

                foreach (var item in lowStockItems)
                {
                    var productName = item.Product?.NameAr ?? $"#{item.ProductId}";
                    var warehouseName = item.Warehouse?.NameAr ?? $"#{item.WarehouseId}";
                    var minStock = item.Product?.MinimumStock ?? 0;

                    _alertService.AddAlert(
                        $"{productName} في {warehouseName} — الرصيد {item.Quantity} أقل من الحد الأدنى {minStock}",
                        "LowStock",
                        AlertSeverity.Warning);
                }

                _logger.LogInformation("Low-stock check: {Count} product(s) below minimum.", lowStockItems.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Low-stock alert job encountered an unhandled error.");
            }
        }

        // ────────────────────────── Helpers ──────────────────────────

        private static Timer CreateTimer(TimeSpan interval, ElapsedEventHandler handler)
        {
            var timer = new Timer(interval.TotalMilliseconds)
            {
                AutoReset = true,
                Enabled = false
            };
            timer.Elapsed += handler;
            return timer;
        }

        private static void StopTimer(ref Timer timer)
        {
            if (timer == null) return;
            timer.Stop();
            timer.Dispose();
            timer = null;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            StopAll();
        }
    }
}
